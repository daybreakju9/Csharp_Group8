using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using System.Collections.Concurrent;

namespace Backend.Services;

public class ImageService : IImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly IImageProcessingService _imageProcessing;
    private readonly IImageGroupService _imageGroupService;
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> QueueLocks = new();

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

    public async Task<(ImageDto Image, bool IsDuplicate)> UploadAsync(int queueId, string folderName, string fileName, Stream fileStream)
    {
        var queueLock = QueueLocks.GetOrAdd(queueId, _ => new SemaphoreSlim(1, 1));
        await queueLock.WaitAsync();

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

            // 同队列按“文件夹+文件名”去重：存在则直接返回
            var existingImage = await _unitOfWork.Images.GetByQueueFolderAndFileNameAsync(queueId, folderName, fileName);
            if (existingImage != null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return (MapToDto(existingImage), true);
            }

            // 提取图片元数据
            var metadata = await _imageProcessing.ExtractMetadataAsync(fileStream, fileName);

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

            return (MapToDto(image), false);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        finally
        {
            queueLock.Release();
        }
    }

    public async Task<UploadResult> UploadBatchAsync(
        int queueId,
        Dictionary<string, List<(string fileName, Stream fileStream)>> folderFiles)
    {
        var result = new UploadResult();

        var queueLock = QueueLocks.GetOrAdd(queueId, _ => new SemaphoreSlim(1, 1));
        await queueLock.WaitAsync();

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

            // 预处理：按文件名分组，提取元数据
            var fileGroups = new Dictionary<string, List<PreparedFile>>();
            foreach (var kvp in folderFiles)
            {
                var folderName = kvp.Key;
                foreach (var (fileName, fileStream) in kvp.Value)
                {
                    var metadata = await _imageProcessing.ExtractMetadataAsync(fileStream, fileName);
                    if (!fileGroups.ContainsKey(fileName))
                    {
                        fileGroups[fileName] = new List<PreparedFile>();
                    }
                    fileGroups[fileName].Add(new PreparedFile
                    {
                        FolderName = folderName,
                        FileName = fileName,
                        Stream = fileStream,
                        Metadata = metadata
                    });
                }
            }

            // 去重：同队列内“文件夹+文件名”重复则跳过
            var allFileKeys = fileGroups.Values
                .SelectMany(v => v.Select(f => $"{f.FolderName}|{f.FileName}"))
                .Distinct();
            var existingFileKeys = await _unitOfWork.Images.GetFolderFileKeysByQueueAsync(queueId, allFileKeys);

            // 处理文件
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

                    var displayOrder = imageGroup.ImageCount;

                    foreach (var prepared in files)
                    {
                        try
                        {
                            var preparedKey = $"{prepared.FolderName}|{prepared.FileName}";
                            if (existingFileKeys.Contains(preparedKey))
                            {
                                result.SkippedCount++;
                                if (result.SkippedFiles.Count < 50)
                                {
                                    result.SkippedFiles.Add($"{prepared.FolderName}/{prepared.FileName}");
                                }
                                continue;
                            }

                            // 保存文件
                            prepared.Stream.Position = 0;
                            var filePath = await _fileStorage.SaveFileAsync(prepared.Stream, prepared.FileName, queueId.ToString());

                            // 创建图片实体
                            var image = new Image
                            {
                                QueueId = queueId,
                                ImageGroupId = imageGroup.Id,
                                FolderName = prepared.FolderName,
                                FileName = prepared.FileName,
                                FilePath = filePath,
                                DisplayOrder = displayOrder++,
                                FileSize = prepared.Metadata.FileSize,
                                Width = prepared.Metadata.Width,
                                Height = prepared.Metadata.Height,
                                FileHash = prepared.Metadata.FileHash,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.Images.AddAsync(image);
                            existingFileKeys.Add(preparedKey);
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"{prepared.FolderName}/{prepared.FileName}: {ex.Message}");
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
        finally
        {
            queueLock.Release();
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

    private class PreparedFile
    {
        public string FolderName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public Stream Stream { get; set; } = Stream.Null;
        public ImageMetadata Metadata { get; set; } = new ImageMetadata();
    }
}
