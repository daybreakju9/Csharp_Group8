using Backend.DTOs;
using Backend.Models;

namespace Backend.Services;

/// <summary>
/// 图片组服务接口
/// </summary>
public interface IImageGroupService
{
    Task<ImageGroup?> GetByIdAsync(int id);
    Task<IEnumerable<ImageGroup>> GetByQueueIdAsync(int queueId);
    Task<ImageGroup?> GetOrCreateAsync(int queueId, string groupName);
    Task<(IEnumerable<ImageGroupDto> Items, int TotalCount)> GetPagedAsync(
        int queueId, int pageNumber, int pageSize, string? searchTerm = null);
    Task<bool> DeleteAsync(int id);
    Task<int> DeleteByQueueIdAsync(int queueId);
}

/// <summary>
/// 图片服务接口
/// </summary>
public interface IImageService
{
    Task<Image?> GetByIdAsync(int id);
    Task<IEnumerable<ImageDto>> GetByQueueIdAsync(int queueId);
    Task<(IEnumerable<ImageDto> Items, int TotalCount)> GetPagedAsync(
        int queueId, int pageNumber, int pageSize, string? searchTerm = null, int? groupId = null);

    /// <summary>
    /// 上传单个图片，返回是否为重复文件（同一队列内）
    /// </summary>
    Task<(ImageDto Image, bool IsDuplicate)> UploadAsync(int queueId, string folderName, string fileName, Stream fileStream);

    /// <summary>
    /// 批量上传图片
    /// </summary>
    Task<UploadResult> UploadBatchAsync(int queueId, Dictionary<string, List<(string fileName, Stream fileStream)>> folderFiles);

    /// <summary>
    /// 删除图片
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 批量删除图片
    /// </summary>
    Task<int> DeleteBatchAsync(IEnumerable<int> ids);

    /// <summary>
    /// 获取下一个未标注的图片组
    /// </summary>
    Task<ImageGroupDto?> GetNextUnannotatedGroupAsync(int queueId, int userId);
}

/// <summary>
/// 上传结果
/// </summary>
public class UploadResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public int TotalGroups { get; set; }
    public int SkippedCount { get; set; }
    public List<string> SkippedFiles { get; set; } = new();
}
