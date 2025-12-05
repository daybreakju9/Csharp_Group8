namespace Backend.Services;

/// <summary>
/// 图片元数据
/// </summary>
public class ImageMetadata
{
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
}

/// <summary>
/// 图片处理服务接口
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// 提取图片元数据
    /// </summary>
    Task<ImageMetadata> ExtractMetadataAsync(Stream imageStream, string fileName);

    /// <summary>
    /// 验证图片文件
    /// </summary>
    Task<bool> IsValidImageAsync(Stream imageStream);
}
