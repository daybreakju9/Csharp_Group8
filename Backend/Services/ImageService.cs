using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ImageService : IImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IImageGroupService _imageGroupService;

    public ImageService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        IImageProcessingService imageProcessing,
        IImageGroupService imageGroupService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _imageProcessing = imageProcessing;
        _imageGroupService = imageGroupService;
    }

    public async Task<Image?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Images.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ImageDto>> GetByQueueIdAsync(int queueId)
    {
        var images = await _unitOfWork.Images.GetByQueueIdAsync(queueId);
        return images.Select(MapToDto);
    }

    public async Task<(IEnumerable<ImageDto> Items, int TotalCount)> GetPagedAsync(
        int queueId, int pageNumber, int pageSize, string? searchTerm = null, int? groupId = null)
    {
        var (items, totalCount) = await _unitOfWork.Images.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: i => i.QueueId == queueId &&
                        (groupId == null || i.ImageGroupId == groupId) &&
                        (string.IsNullOrEmpty(searchTerm) ||
                         i.FileName.Contains(searchTerm) ||
                         i.FolderName.Contains(searchTerm)),
            orderBy: q => q.OrderBy(i => i.ImageGroupId).ThenBy(i => i.DisplayOrder),
            includeProperties: "ImageGroup"
        );

        var dtos = items.Select(MapToDto);
        return (dtos, totalCount);
    }

    public async Task<ImageDto> UploadAsync(int queueId, string folderName, string fileName, Stream fileStream)
    {
        // 开始事务
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 验证队列存在
            var queue = await _unitOfWork.Queues.GetByIdAsync(queueId);
            if (queue == null)
            {
                throw new ArgumentException("队列不存在");
            }

            // 提取图片元数据
            var metadata = await _imageProcessing.ExtractMetadataAsync(fileStream, fileName);

            // 检查是否已存在相同哈希的图片（去重）
            var existingImage = await _unitOfWork.Images.GetByFileHashAsync(metadata.FileHash);
            if (existingImage != null)
            {
                throw new InvalidOperationException($"图片已存在: {existingImage.FileName}");
            }

            // 保存文件
            fileStream.Position = 0;
            var filePath = await _fileStorage.SaveFileAsync(fileStream, fileName, queueId.ToString());

            // 获取或创建图片组
            var imageGroup = await _imageGroupService.GetOrCreateAsync(queueId, fileName);
            if (imageGroup == null)
            {
                throw new Exception("无法创建图片组");
            }

            // 获取当前组中的图片数量以确定 DisplayOrder
            var imagesInGroup = await _unitOfWork.Images.GetByImageGroupIdAsync(imageGroup.Id);
            var displayOrder = imagesInGroup.Count();

            // 创建图片实体
            var image = new Image
            {
                QueueId = queueId,
                ImageGroupId = imageGroup.Id,
                FolderName = folderName,
                FileName = fileName,
                FilePath = filePath,
                DisplayOrder = displayOrder,
                FileSize = metadata.FileSize,
                Width = metadata.Width,
                Height = metadata.Height,
                FileHash = metadata.FileHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Images.AddAsync(image);

            // 更新图片组的图片数量
            imageGroup.ImageCount = displayOrder + 1;
            imageGroup.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.ImageGroups.Update(imageGroup);

            // 更新队列统计
            await UpdateQueueStatisticsAsync(queueId);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return MapToDto(image);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<UploadResult> UploadBatchAsync(
        int queueId,
        Dictionary<string, List<(string fileName, Stream fileStream)>> folderFiles)
    {
        var result = new UploadResult();

        // 开始事务
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 验证队列存在
            var queue = await _unitOfWork.Queues.GetByIdAsync(queueId);
            if (queue == null)
            {
                throw new ArgumentException("队列不存在");
            }

            // 按文件名分组（同名文件来自不同文件夹）
            var fileGroups = new Dictionary<string, List<(string folderName, string fileName, Stream fileStream)>>();

            foreach (var kvp in folderFiles)
            {
                var folderName = kvp.Key;
                foreach (var (fileName, fileStream) in kvp.Value)
                {
                    if (!fileGroups.ContainsKey(fileName))
                    {
                        fileGroups[fileName] = new List<(string, string, Stream)>();
                    }
                    fileGroups[fileName].Add((folderName, fileName, fileStream));
                }
            }

            // 按组处理文件
            foreach (var group in fileGroups)
            {
                var groupName = group.Key;
                var files = group.Value;

                try
                {
                    // 获取或创建图片组
                    var imageGroup = await _imageGroupService.GetOrCreateAsync(queueId, groupName);
                    if (imageGroup == null)
                    {
                        result.Errors.Add($"无法为 {groupName} 创建图片组");
                        result.FailureCount += files.Count;
                        continue;
                    }

                    // 上传组内的每个文件
                    var displayOrder = imageGroup.ImageCount;

                    foreach (var (folderName, fileName, fileStream) in files)
                    {
                        try
                        {
                            // 提取元数据
                            var metadata = await _imageProcessing.ExtractMetadataAsync(fileStream, fileName);

                            // 保存文件
                            fileStream.Position = 0;
                            var filePath = await _fileStorage.SaveFileAsync(fileStream, fileName, queueId.ToString());

                            // 创建图片实体
                            var image = new Image
                            {
                                QueueId = queueId,
                                ImageGroupId = imageGroup.Id,
                                FolderName = folderName,
                                FileName = fileName,
                                FilePath = filePath,
                                DisplayOrder = displayOrder++,
                                FileSize = metadata.FileSize,
                                Width = metadata.Width,
                                Height = metadata.Height,
                                FileHash = metadata.FileHash,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.Images.AddAsync(image);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"{folderName}/{fileName}: {ex.Message}");
                            result.FailureCount++;
                        }
                    }

                    // 更新图片组的图片数量
                    imageGroup.ImageCount = displayOrder;
                    imageGroup.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.ImageGroups.Update(imageGroup);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"处理组 {groupName} 失败: {ex.Message}");
                    result.FailureCount += files.Count;
                }
            }

            // 更新队列统计
            await UpdateQueueStatisticsAsync(queueId);

            // 设置总组数
            result.TotalGroups = fileGroups.Count;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Errors.Add($"批量上传失败: {ex.Message}");
            return result;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var image = await _unitOfWork.Images.GetByIdAsync(id);
        if (image == null)
        {
            return false;
        }

        // 开始事务
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 删除文件
            await _fileStorage.DeleteFileAsync(image.FilePath);

            // 软删除图片
            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow;
            _unitOfWork.Images.Update(image);

            // 更新图片组统计
            var imageGroup = await _unitOfWork.ImageGroups.GetByIdAsync(image.ImageGroupId);
            if (imageGroup != null)
            {
                var remainingImages = await _unitOfWork.Images.CountAsync(
                    i => i.ImageGroupId == imageGroup.Id && !i.IsDeleted);
                imageGroup.ImageCount = remainingImages;
                imageGroup.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.ImageGroups.Update(imageGroup);
            }

            // 更新队列统计
            await UpdateQueueStatisticsAsync(image.QueueId);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<int> DeleteBatchAsync(IEnumerable<int> ids)
    {
        var count = 0;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var id in ids)
            {
                if (await DeleteAsync(id))
                {
                    count++;
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            return count;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ImageGroupDto?> GetNextUnannotatedGroupAsync(int queueId, int userId)
    {
        // 获取队列的所有图片组
        var allGroups = await _unitOfWork.ImageGroups.GetByQueueIdAsync(queueId);

        if (!allGroups.Any())
        {
            return null;
        }

        // 获取用户已标注的图片组ID
        var annotatedGroupIds = (await _unitOfWork.SelectionRecords.GetByUserIdAsync(userId))
            .Where(s => s.QueueId == queueId)
            .Select(s => s.ImageGroupId)
            .ToHashSet();

        // 找到第一个未标注的图片组
        var nextGroup = allGroups
            .Where(g => !annotatedGroupIds.Contains(g.Id))
            .OrderBy(g => g.DisplayOrder)
            .FirstOrDefault();

        if (nextGroup == null)
        {
            return null;
        }

        // 获取该组的所有图片
        var images = await _unitOfWork.Images.GetByImageGroupIdAsync(nextGroup.Id);

        return new ImageGroupDto
        {
            Id = nextGroup.Id,
            QueueId = nextGroup.QueueId,
            GroupName = nextGroup.GroupName,
            DisplayOrder = nextGroup.DisplayOrder,
            ImageCount = nextGroup.ImageCount,
            IsCompleted = nextGroup.IsCompleted,
            CreatedAt = nextGroup.CreatedAt,
            Images = images.Select(MapToDto).ToList()
        };
    }

    private async Task UpdateQueueStatisticsAsync(int queueId)
    {
        var queue = await _unitOfWork.Queues.GetByIdAsync(queueId);
        if (queue != null)
        {
            queue.GroupCount = await _unitOfWork.ImageGroups.CountAsync(g => g.QueueId == queueId);
            queue.TotalImageCount = await _unitOfWork.Images.CountAsync(i => i.QueueId == queueId);
            queue.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Queues.Update(queue);
        }
    }

    private static ImageDto MapToDto(Image image)
    {
        return new ImageDto
        {
            Id = image.Id,
            QueueId = image.QueueId,
            ImageGroupId = image.ImageGroupId,
            FolderName = image.FolderName,
            FileName = image.FileName,
            FilePath = image.FilePath,
            DisplayOrder = image.DisplayOrder,
            FileSize = image.FileSize,
            Width = image.Width,
            Height = image.Height,
            FileHash = image.FileHash,
            CreatedAt = image.CreatedAt
        };
    }
}
