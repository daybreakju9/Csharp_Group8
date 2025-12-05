using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace Backend.Services;

/// <summary>
/// 图片处理服务实现（使用 ImageSharp）
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly IFileStorageService _fileStorage;

    public ImageProcessingService(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<ImageMetadata> ExtractMetadataAsync(Stream imageStream, string fileName)
    {
        var metadata = new ImageMetadata();

        // 保存原始位置
        var originalPosition = imageStream.Position;

        try
        {
            // 加载图片以获取尺寸
            using var image = await Image.LoadAsync(imageStream);
            metadata.Width = image.Width;
            metadata.Height = image.Height;

            // 重置流位置以计算哈希
            imageStream.Position = 0;
            metadata.FileHash = await _fileStorage.ComputeFileHashAsync(imageStream);

            // 重置流位置以获取文件大小
            imageStream.Position = 0;
            metadata.FileSize = imageStream.Length;
        }
        catch
        {
            // 如果无法加载图片，仅计算哈希和大小
            imageStream.Position = 0;
            metadata.FileHash = await _fileStorage.ComputeFileHashAsync(imageStream);
            imageStream.Position = 0;
            metadata.FileSize = imageStream.Length;
            metadata.Width = 0;
            metadata.Height = 0;
        }
        finally
        {
            // 恢复原始位置
            imageStream.Position = originalPosition;
        }

        return metadata;
    }

    public async Task<bool> IsValidImageAsync(Stream imageStream)
    {
        try
        {
            var originalPosition = imageStream.Position;
            using var image = await Image.LoadAsync(imageStream);
            imageStream.Position = originalPosition;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
