using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IWebHostEnvironment env, IConfiguration config)
    {
        // 1. 从配置读取路径
        var uploadRoot = config["Storage:UploadRoot"];

        // 2. 处理相对路径
        if (!Path.IsPathFullyQualified(uploadRoot))
        {
            uploadRoot = Path.Combine(env.ContentRootPath, uploadRoot);
        }

        // 3. 自动创建目录
        if (!Directory.Exists(uploadRoot))
        {
            Directory.CreateDirectory(uploadRoot);
            Console.WriteLine("[Storage] Upload directory created at: " + uploadRoot);
        }

        _root = uploadRoot;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var folderPath = Path.Combine(_root, folder);
        Directory.CreateDirectory(folderPath);

        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream);

        return $"/uploads/{folder}/{uniqueName}";
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var relative = filePath.Replace("/uploads/", "").TrimStart('/');
        var fullPath = Path.Combine(_root, relative);

        if (!File.Exists(fullPath))
            return false;

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
        var relative = filePath.Replace("/uploads/", "").TrimStart('/');
        var fullPath = Path.Combine(_root, relative);

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var relative = filePath.Replace("/uploads/", "").TrimStart('/');
        var fullPath = Path.Combine(_root, relative);

        return Task.FromResult(File.Exists(fullPath));
    }

    public async Task<string> ComputeFileHashAsync(Stream fileStream)
    {
        using var md5 = MD5.Create();
        var hash = await md5.ComputeHashAsync(fileStream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
