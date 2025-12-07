using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// 选择记录服务实现
/// </summary>
public class SelectionService : ISelectionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SelectionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SelectionDto> CreateAsync(CreateSelectionDto createDto, int userId)
    {
        // 开始事务
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 检查用户是否存在
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("用户不存在");
            }

            // Guest用户不能参与标注
            if (user.Role == "Guest")
            {
                throw new UnauthorizedAccessException("Guest用户不能参与标注");
            }

            // 检查队列是否存在
            var queueExists = await _unitOfWork.Queues.GetByIdAsync(createDto.QueueId);
            if (queueExists == null)
            {
                throw new ArgumentException("队列不存在");
            }

            // 检查图片是否存在且属于指定队列和组
            var image = await _unitOfWork.Images.GetByIdAsync(createDto.SelectedImageId);
            if (image == null || image.QueueId != createDto.QueueId || image.ImageGroupId != createDto.ImageGroupId)
            {
                throw new ArgumentException("图片不存在或不属于指定队列");
            }

            // 检查是否已经选择过这个图片组（防止并发）
            var existingSelection = await _unitOfWork.SelectionRecords
                .GetByUserAndGroupAsync(userId, createDto.ImageGroupId);
            if (existingSelection != null && existingSelection.QueueId == createDto.QueueId)
            {
                throw new InvalidOperationException("您已经选择过这个图片组");
            }

            // 创建选择记录
            var selection = new SelectionRecord
            {
                QueueId = createDto.QueueId,
                UserId = userId,
                ImageGroupId = createDto.ImageGroupId,
                SelectedImageId = createDto.SelectedImageId,
                DurationSeconds = createDto.DurationSeconds,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SelectionRecords.AddAsync(selection);

            // 更新或创建用户进度
            var progress = await _unitOfWork.UserProgresses
                .GetByUserAndQueueAsync(userId, createDto.QueueId);

            if (progress == null)
            {
                // 获取总组数
                var totalGroups = await _unitOfWork.ImageGroups
                    .CountAsync(g => g.QueueId == createDto.QueueId);

                progress = new UserProgress
                {
                    QueueId = createDto.QueueId,
                    UserId = userId,
                    CompletedGroups = 1,
                    TotalGroups = totalGroups,
                    LastUpdated = DateTime.UtcNow
                };
                await _unitOfWork.UserProgresses.AddAsync(progress);
            }
            else
            {
                progress.CompletedGroups++;
                progress.LastUpdated = DateTime.UtcNow;
                _unitOfWork.UserProgresses.Update(progress);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // 重新加载以获取导航属性
            var createdSelection = await _unitOfWork.SelectionRecords.GetByIdAsync(selection.Id);
            if (createdSelection == null)
            {
                throw new InvalidOperationException("选择记录创建失败");
            }

            return await MapToDtoAsync(createdSelection);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<SelectionDto?> GetByIdAsync(int id)
    {
        var selection = await _unitOfWork.SelectionRecords.GetByIdAsync(id);
        if (selection == null)
        {
            return null;
        }

        return await MapToDtoAsync(selection);
    }

    public async Task<IEnumerable<SelectionDto>> GetByQueueIdAsync(int queueId, int? userId = null, bool isAdmin = false)
    {
        IEnumerable<SelectionRecord> selections;

        if (userId.HasValue)
        {
            // 获取指定用户的选择记录
            var userSelections = await _unitOfWork.SelectionRecords.GetByUserIdAsync(userId.Value);
            selections = userSelections.Where(s => s.QueueId == queueId);
        }
        else if (isAdmin)
        {
            // 管理员可以查看所有选择记录
            var queueSelections = await _unitOfWork.SelectionRecords.GetByQueueIdAsync(queueId);
            selections = queueSelections;
        }
        else
        {
            // 非管理员用户只能查看自己的选择记录
            throw new UnauthorizedAccessException("无权访问其他用户的选择记录");
        }

        var dtos = new List<SelectionDto>();
        foreach (var selection in selections)
        {
            dtos.Add(await MapToDtoAsync(selection));
        }

        return dtos.OrderBy(s => s.CreatedAt);
    }

    public async Task<UserProgressDto> GetProgressAsync(int queueId, int userId)
    {
        var progress = await _unitOfWork.UserProgresses.GetByUserAndQueueAsync(userId, queueId);

        if (progress == null)
        {
            // 创建初始进度
            var totalGroups = await _unitOfWork.ImageGroups
                .CountAsync(g => g.QueueId == queueId);

            var queue = await _unitOfWork.Queues.GetByIdAsync(queueId);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            return new UserProgressDto
            {
                QueueId = queueId,
                QueueName = queue?.Name ?? string.Empty,
                UserId = userId,
                Username = user?.Username ?? string.Empty,
                CompletedGroups = 0,
                TotalGroups = totalGroups,
                ProgressPercentage = 0,
                LastUpdated = DateTime.UtcNow
            };
        }

        var queueInfo = await _unitOfWork.Queues.GetByIdAsync(queueId);
        var userInfo = await _unitOfWork.Users.GetByIdAsync(userId);

        return new UserProgressDto
        {
            QueueId = progress.QueueId,
            QueueName = queueInfo?.Name ?? string.Empty,
            UserId = progress.UserId,
            Username = userInfo?.Username ?? string.Empty,
            CompletedGroups = progress.CompletedGroups,
            TotalGroups = progress.TotalGroups,
            ProgressPercentage = progress.TotalGroups > 0
                ? (decimal)progress.CompletedGroups / progress.TotalGroups * 100
                : 0,
            LastUpdated = progress.LastUpdated
        };
    }

    public async Task<IEnumerable<UserProgressDto>> GetAllProgressAsync(int? queueId = null)
    {
        IEnumerable<UserProgress> progresses;

        if (queueId.HasValue)
        {
            progresses = await _unitOfWork.UserProgresses.GetByQueueIdAsync(queueId.Value);
        }
        else
        {
            // 获取所有进度（需要实现GetAllAsync或使用其他方法）
            var allProgresses = await _unitOfWork.UserProgresses.GetAllAsync();
            progresses = allProgresses;
        }

        var dtos = new List<UserProgressDto>();

        foreach (var progress in progresses.OrderByDescending(p => p.LastUpdated))
        {
            var queue = await _unitOfWork.Queues.GetByIdAsync(progress.QueueId);
            var user = await _unitOfWork.Users.GetByIdAsync(progress.UserId);

            dtos.Add(new UserProgressDto
            {
                QueueId = progress.QueueId,
                QueueName = queue?.Name ?? string.Empty,
                UserId = progress.UserId,
                Username = user?.Username ?? string.Empty,
                CompletedGroups = progress.CompletedGroups,
                TotalGroups = progress.TotalGroups,
                ProgressPercentage = progress.TotalGroups > 0
                    ? (decimal)progress.CompletedGroups / progress.TotalGroups * 100
                    : 0,
                LastUpdated = progress.LastUpdated
            });
        }

        return dtos;
    }

    private async Task<SelectionDto> MapToDtoAsync(SelectionRecord selection)
    {
        // 需要加载导航属性
        var user = await _unitOfWork.Users.GetByIdAsync(selection.UserId);
        var image = await _unitOfWork.Images.GetByIdAsync(selection.SelectedImageId);
        var imageGroup = await _unitOfWork.ImageGroups.GetByIdAsync(selection.ImageGroupId);

        if (user == null || image == null || imageGroup == null)
        {
            throw new InvalidOperationException("无法加载选择记录的关联数据");
        }

        return new SelectionDto
        {
            Id = selection.Id,
            QueueId = selection.QueueId,
            UserId = selection.UserId,
            Username = user.Username,
            ImageGroupId = selection.ImageGroupId,
            ImageGroupName = imageGroup.GroupName,
            SelectedImageId = selection.SelectedImageId,
            SelectedImagePath = image.FilePath,
            SelectedFolderName = image.FolderName,
            DurationSeconds = selection.DurationSeconds,
            CreatedAt = selection.CreatedAt
        };
    }
}

