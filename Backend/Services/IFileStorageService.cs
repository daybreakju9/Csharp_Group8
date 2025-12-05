namespace Backend.Services;

/// <summary>
/// 文件存储服务接口
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// 保存文件
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// 删除文件
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// 获取文件
    /// </summary>
    Task<byte[]?> GetFileAsync(string filePath);

    /// <summary>
    /// 文件是否存在
    /// </summary>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// 计算文件MD5
    /// </summary>
    Task<string> ComputeFileHashAsync(Stream fileStream);
}
