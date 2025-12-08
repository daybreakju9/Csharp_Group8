using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// 队列服务实现
/// </summary>
public class QueueService : IQueueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public QueueService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<QueueDto>> GetAllAsync(int? projectId = null)
    {
        var queues = (projectId.HasValue
            ? await _unitOfWork.Queues.GetByProjectIdAsync(projectId.Value)
            : await _unitOfWork.Queues.GetAllAsync())
            .ToList();

        if (queues.Count == 0)
        {
            return Enumerable.Empty<QueueDto>();
        }

        var projectIds = queues.Select(q => q.ProjectId).Distinct().ToList();
        var projects = await _unitOfWork.Projects.FindAsync(p => projectIds.Contains(p.Id));
        var projectNameLookup = projects.ToDictionary(p => p.Id, p => p.Name);

        var queueDtos = new List<QueueDto>();

        foreach (var queue in queues)
        {
            projectNameLookup.TryGetValue(queue.ProjectId, out var projectName);
            queueDtos.Add(new QueueDto
            {
                Id = queue.Id,
                ProjectId = queue.ProjectId,
                ProjectName = projectName ?? string.Empty,
                Name = queue.Name,
                Description = queue.Description,
                ComparisonCount = queue.ComparisonCount,
                GroupCount = queue.GroupCount,
                TotalImageCount = queue.TotalImageCount,
                Status = queue.Status,
                IsRandomOrder = queue.IsRandomOrder,
                CreatedAt = queue.CreatedAt,
                UpdatedAt = queue.UpdatedAt
            });
        }

        return queueDtos;
    }

    public async Task<QueueDto?> GetByIdAsync(int id)
    {
        var queue = await _unitOfWork.Queues.GetByIdAsync(id);
        if (queue == null)
        {
            return null;
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(queue.ProjectId);

        return new QueueDto
        {
            Id = queue.Id,
            ProjectId = queue.ProjectId,
            ProjectName = project?.Name ?? string.Empty,
            Name = queue.Name,
            Description = queue.Description,
            ComparisonCount = queue.ComparisonCount,
            GroupCount = queue.GroupCount,
            TotalImageCount = queue.TotalImageCount,
            Status = queue.Status,
            IsRandomOrder = queue.IsRandomOrder,
            CreatedAt = queue.CreatedAt,
            UpdatedAt = queue.UpdatedAt
        };
    }

    public async Task<QueueDto> CreateAsync(CreateQueueDto createDto)
    {
        // 验证项目是否存在
        var projectExists = await _unitOfWork.Projects.GetByIdAsync(createDto.ProjectId);
        if (projectExists == null)
        {
            throw new ArgumentException("项目不存在");
        }

        var queue = new Queue
        {
            ProjectId = createDto.ProjectId,
            Name = createDto.Name,
            Description = createDto.Description,
            ComparisonCount = createDto.ComparisonCount,
            GroupCount = 0,
            TotalImageCount = 0,
            Status = QueueStatus.Draft,
            IsRandomOrder = createDto.IsRandomOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Queues.AddAsync(queue);
        await _unitOfWork.SaveChangesAsync();

        // 重新加载以获取项目信息
        var createdQueue = await _unitOfWork.Queues.GetByIdAsync(queue.Id);
        if (createdQueue == null)
        {
            throw new InvalidOperationException("队列创建失败");
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(createdQueue.ProjectId);

        return new QueueDto
        {
            Id = createdQueue.Id,
            ProjectId = createdQueue.ProjectId,
            ProjectName = project?.Name ?? string.Empty,
            Name = createdQueue.Name,
            Description = createdQueue.Description,
            ComparisonCount = createdQueue.ComparisonCount,
            GroupCount = createdQueue.GroupCount,
            TotalImageCount = createdQueue.TotalImageCount,
            Status = createdQueue.Status,
            IsRandomOrder = createdQueue.IsRandomOrder,
            CreatedAt = createdQueue.CreatedAt,
            UpdatedAt = createdQueue.UpdatedAt
        };
    }

    public async Task<QueueDto?> UpdateAsync(int id, UpdateQueueDto updateDto)
    {
        var queue = await _unitOfWork.Queues.GetByIdAsync(id);
        if (queue == null)
        {
            return null;
        }

        queue.Name = updateDto.Name;
        queue.Description = updateDto.Description;
        queue.ComparisonCount = updateDto.ComparisonCount;

        if (!string.IsNullOrEmpty(updateDto.Status))
        {
            queue.Status = updateDto.Status;
        }

        if (updateDto.IsRandomOrder.HasValue)
        {
            queue.IsRandomOrder = updateDto.IsRandomOrder.Value;
        }

        queue.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Queues.Update(queue);
        await _unitOfWork.SaveChangesAsync();

        // 重新加载以获取项目信息
        var updatedQueue = await _unitOfWork.Queues.GetByIdAsync(id);
        if (updatedQueue == null)
        {
            return null;
        }

        var project = await _unitOfWork.Projects.GetByIdAsync(updatedQueue.ProjectId);

        return new QueueDto
        {
            Id = updatedQueue.Id,
            ProjectId = updatedQueue.ProjectId,
            ProjectName = project?.Name ?? string.Empty,
            Name = updatedQueue.Name,
            Description = updatedQueue.Description,
            ComparisonCount = updatedQueue.ComparisonCount,
            GroupCount = updatedQueue.GroupCount,
            TotalImageCount = updatedQueue.TotalImageCount,
            Status = updatedQueue.Status,
            IsRandomOrder = updatedQueue.IsRandomOrder,
            CreatedAt = updatedQueue.CreatedAt,
            UpdatedAt = updatedQueue.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var queue = await _unitOfWork.Queues.GetByIdAsync(id);
        if (queue == null)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;

            // 删除文件并软删图片
            var images = await _unitOfWork.Images.GetByQueueIdAsync(id);
            foreach (var image in images)
            {
                await _fileStorageService.DeleteFileAsync(image.FilePath);
                image.IsDeleted = true;
                image.DeletedAt = now;
                _unitOfWork.Images.Update(image);
            }

            // 软删图片组
            var groups = await _unitOfWork.ImageGroups.GetByQueueIdAsync(id);
            foreach (var group in groups)
            {
                group.IsDeleted = true;
                group.DeletedAt = now;
                _unitOfWork.ImageGroups.Update(group);
            }

            // 删除选择记录与进度（无软删字段，直接移除）
            var selections = await _unitOfWork.SelectionRecords.GetByQueueIdAsync(id);
            _unitOfWork.SelectionRecords.RemoveRange(selections);

            var progresses = await _unitOfWork.UserProgresses.GetByQueueIdAsync(id);
            _unitOfWork.UserProgresses.RemoveRange(progresses);

            // 最后软删队列并清零统计
            queue.IsDeleted = true;
            queue.DeletedAt = now;
            queue.GroupCount = 0;
            queue.TotalImageCount = 0;
            queue.UpdatedAt = now;
            _unitOfWork.Queues.Update(queue);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return true;
    }
}

