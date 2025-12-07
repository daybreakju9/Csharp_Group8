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

    public QueueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<QueueDto>> GetAllAsync(int? projectId = null)
    {
        var queues = projectId.HasValue
            ? await _unitOfWork.Queues.GetByProjectIdAsync(projectId.Value)
            : await _unitOfWork.Queues.GetAllAsync();

        var queueDtos = new List<QueueDto>();

        foreach (var queue in queues)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(queue.ProjectId);
            queueDtos.Add(new QueueDto
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

        // 使用软删除
        queue.IsDeleted = true;
        queue.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Queues.Update(queue);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

