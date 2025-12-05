using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ImageGroupService : IImageGroupService
{
    private readonly IUnitOfWork _unitOfWork;

    public ImageGroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ImageGroup?> GetByIdAsync(int id)
    {
        return await _unitOfWork.ImageGroups.GetWithImagesAsync(id);
    }

    public async Task<IEnumerable<ImageGroup>> GetByQueueIdAsync(int queueId)
    {
        return await _unitOfWork.ImageGroups.GetByQueueIdAsync(queueId);
    }

    public async Task<ImageGroup?> GetOrCreateAsync(int queueId, string groupName)
    {
        // 检查是否已存在
        var existing = await _unitOfWork.ImageGroups.GetByNameAsync(queueId, groupName);
        if (existing != null)
        {
            return existing;
        }

        // 创建新的图片组
        var maxOrder = (await _unitOfWork.ImageGroups.GetByQueueIdAsync(queueId))
            .MaxBy(g => g.DisplayOrder)?.DisplayOrder ?? 0;

        var newGroup = new ImageGroup
        {
            QueueId = queueId,
            GroupName = groupName,
            DisplayOrder = maxOrder + 1,
            ImageCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ImageGroups.AddAsync(newGroup);
        await _unitOfWork.SaveChangesAsync();

        return newGroup;
    }

    public async Task<(IEnumerable<ImageGroupDto> Items, int TotalCount)> GetPagedAsync(
        int queueId, int pageNumber, int pageSize, string? searchTerm = null)
    {
        var (items, totalCount) = await _unitOfWork.ImageGroups.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: g => g.QueueId == queueId &&
                        (string.IsNullOrEmpty(searchTerm) || g.GroupName.Contains(searchTerm)),
            orderBy: q => q.OrderBy(g => g.DisplayOrder),
            includeProperties: "Images"
        );

        var dtos = items.Select(g => new ImageGroupDto
        {
            Id = g.Id,
            QueueId = g.QueueId,
            GroupName = g.GroupName,
            DisplayOrder = g.DisplayOrder,
            ImageCount = g.ImageCount,
            IsCompleted = g.IsCompleted,
            CreatedAt = g.CreatedAt,
            Images = g.Images.Select(i => new ImageDto
            {
                Id = i.Id,
                QueueId = i.QueueId,
                ImageGroupId = i.ImageGroupId,
                FolderName = i.FolderName,
                FileName = i.FileName,
                FilePath = i.FilePath,
                DisplayOrder = i.DisplayOrder,
                FileSize = i.FileSize,
                Width = i.Width,
                Height = i.Height
            }).ToList()
        });

        return (dtos, totalCount);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var group = await _unitOfWork.ImageGroups.GetByIdAsync(id);
        if (group == null)
        {
            return false;
        }

        // 软删除
        group.IsDeleted = true;
        group.DeletedAt = DateTime.UtcNow;
        _unitOfWork.ImageGroups.Update(group);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteByQueueIdAsync(int queueId)
    {
        var groups = await _unitOfWork.ImageGroups.GetByQueueIdAsync(queueId);
        var count = 0;

        foreach (var group in groups)
        {
            group.IsDeleted = true;
            group.DeletedAt = DateTime.UtcNow;
            _unitOfWork.ImageGroups.Update(group);
            count++;
        }

        await _unitOfWork.SaveChangesAsync();
        return count;
    }
}
