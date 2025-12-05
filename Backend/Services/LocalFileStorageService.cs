using System.Security.Cryptography;

namespace Backend.Services;

/// <summary>
/// 本地文件存储服务实现
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const string UPLOAD_FOLDER = "uploads";

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        // 创建上传目录
        var uploadPath = Path.Combine(_environment.ContentRootPath, UPLOAD_FOLDER, folder);
        Directory.CreateDirectory(uploadPath);

        // 生成唯一文件名
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // 保存文件
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // 返回相对路径
        return $"/{UPLOAD_FOLDER}/{folder}/{uniqueFileName}";
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));

        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            await Task.Run(() => File.Delete(fullPath));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]?> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));

        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));
        return Task.FromResult(File.Exists(fullPath));
    }

    public async Task<string> ComputeFileHashAsync(Stream fileStream)
    {
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(fileStream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
