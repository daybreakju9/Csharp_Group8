using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ImagesController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpGet("queue/{queueId}")]
    public async Task<ActionResult<List<ImageDto>>> GetQueueImages(int queueId)
    {
        var images = await _context.Images
            .Where(i => i.QueueId == queueId)
            .OrderBy(i => i.ImageGroup)
            .ThenBy(i => i.Order)
            .Select(i => new ImageDto
            {
                Id = i.Id,
                QueueId = i.QueueId,
                ImageGroup = i.ImageGroup,
                FolderName = i.FolderName,
                FileName = i.FileName,
                FilePath = i.FilePath,
                Order = i.Order
            })
            .ToListAsync();

        return Ok(images);
    }

    [HttpGet("next-group/{queueId}")]
    public async Task<ActionResult<ImageGroupDto>> GetNextImageGroup(int queueId)
    {
        var userId = GetUserId();

        // Check if user is a Guest (not allowed to participate in annotation)
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { message = "用户不存在" });
        }

        if (user.Role == "Guest")
        {
            return Forbid();  // 403 Forbidden - Guest users cannot access annotation images
        }

        // Get all image groups for the queue
        var allGroups = await _context.Images
            .Where(i => i.QueueId == queueId)
            .Select(i => i.ImageGroup)
            .Distinct()
            .ToListAsync();

        if (!allGroups.Any())
        {
            return NotFound(new { message = "该队列没有图片" });
        }

        // Get groups already selected by this user
        var selectedGroups = await _context.SelectionRecords
            .Where(s => s.QueueId == queueId && s.UserId == userId)
            .Select(s => s.ImageGroup)
            .ToListAsync();

        // Find the next unselected group
        var nextGroup = allGroups.FirstOrDefault(g => !selectedGroups.Contains(g));

        if (nextGroup == null)
        {
            return Ok(new { message = "所有图片组已完成选择", completed = true });
        }

        // Get images for this group
        var images = await _context.Images
            .Where(i => i.QueueId == queueId && i.ImageGroup == nextGroup)
            .OrderBy(i => i.Order)
            .Select(i => new ImageDto
            {
                Id = i.Id,
                QueueId = i.QueueId,
                ImageGroup = i.ImageGroup,
                FolderName = i.FolderName,
                FileName = i.FileName,
                FilePath = i.FilePath,
                Order = i.Order
            })
            .ToListAsync();

        return Ok(new ImageGroupDto
        {
            ImageGroup = nextGroup,
            Images = images
        });
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UploadImage([FromForm] int queueId, [FromForm] string folderName, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "没有上传文件" });
        }

        if (string.IsNullOrWhiteSpace(folderName))
        {
            return BadRequest(new { message = "文件夹名称不能为空" });
        }

        // Start transaction for data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var queue = await _context.Queues.FindAsync(queueId);
            if (queue == null)
            {
                return NotFound(new { message = "队列不存在" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", queueId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Save file
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Find existing images with the same ImageGroup (filename) to determine Order
            var existingImagesInGroup = await _context.Images
                .Where(i => i.QueueId == queueId && i.ImageGroup == file.FileName)
                .ToListAsync();

            int order = existingImagesInGroup.Count;

            // Create image entity
            var image = new Image
            {
                QueueId = queueId,
                ImageGroup = file.FileName,
                FolderName = folderName,
                FileName = file.FileName,
                FilePath = $"/uploads/{queueId}/{uniqueFileName}",
                Order = order,
                CreatedAt = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            // Update queue total images count (count unique ImageGroups)
            var totalGroups = await _context.Images
                .Where(i => i.QueueId == queueId)
                .Select(i => i.ImageGroup)
                .Distinct()
                .CountAsync();
            queue.TotalImages = totalGroups;
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "上传成功",
                imageId = image.Id,
                fileName = file.FileName
            });
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "上传图片时发生错误", details = ex.Message });
        }
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportImages([FromForm] int queueId, [FromForm] List<IFormFile> files, [FromForm] List<string> folderNames)
    {
        if (files == null || !files.Any())
        {
            return BadRequest(new { message = "没有上传文件" });
        }

        if (files.Count != folderNames.Count)
        {
            return BadRequest(new { message = "文件和文件夹名称数量不匹配" });
        }

        // Start transaction for data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var queue = await _context.Queues.FindAsync(queueId);
            if (queue == null)
            {
                return NotFound(new { message = "队列不存在" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", queueId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Group files by filename (same-name files from different folders)
            var fileGroups = new Dictionary<string, List<(IFormFile file, string folderName, int index)>>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var folderName = folderNames[i];
                var fileName = file.FileName;

                if (!fileGroups.ContainsKey(fileName))
                {
                    fileGroups[fileName] = new List<(IFormFile, string, int)>();
                }

                fileGroups[fileName].Add((file, folderName, i));
            }

            var imageEntities = new List<Image>();

            foreach (var group in fileGroups)
            {
                var fileName = group.Key;
                var filesInGroup = group.Value;

                int order = 0;
                foreach (var (file, folderName, _) in filesInGroup)
                {
                    // Save file
                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create image entity
                    var image = new Image
                    {
                        QueueId = queueId,
                        ImageGroup = fileName,
                        FolderName = folderName,
                        FileName = file.FileName,
                        FilePath = $"/uploads/{queueId}/{uniqueFileName}",
                        Order = order++,
                        CreatedAt = DateTime.UtcNow
                    };

                    imageEntities.Add(image);
                }
            }

            _context.Images.AddRange(imageEntities);
            await _context.SaveChangesAsync();

            // Update queue total images (count unique ImageGroups)
            var totalGroups = await _context.Images
                .Where(i => i.QueueId == queueId)
                .Select(i => i.ImageGroup)
                .Distinct()
                .CountAsync();
            queue.TotalImages = totalGroups;
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return Ok(new { message = $"成功导入 {imageEntities.Count} 张图片，共 {totalGroups} 个图片组" });
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "导入图片时发生错误", details = ex.Message });
        }
    }

    [HttpPost("import-single")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ImportSingleImage([FromForm] int queueId, [FromForm] IFormFile file, [FromForm] string folderName)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "没有上传文件" });
        }

        if (string.IsNullOrWhiteSpace(folderName))
        {
            return BadRequest(new { message = "文件夹名称不能为空" });
        }

        // Start transaction for data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var queue = await _context.Queues.FindAsync(queueId);
            if (queue == null)
            {
                return NotFound(new { message = "队列不存在" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", queueId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Save file
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Find existing images with the same ImageGroup (filename) to determine Order
            var existingImagesInGroup = await _context.Images
                .Where(i => i.QueueId == queueId && i.ImageGroup == file.FileName)
                .ToListAsync();

            int order = existingImagesInGroup.Count;

            // Create image entity
            var image = new Image
            {
                QueueId = queueId,
                ImageGroup = file.FileName,
                FolderName = folderName,
                FileName = file.FileName,
                FilePath = $"/uploads/{queueId}/{uniqueFileName}",
                Order = order,
                CreatedAt = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            // Update queue total images count (count unique ImageGroups)
            var totalGroups = await _context.Images
                .Where(i => i.QueueId == queueId)
                .Select(i => i.ImageGroup)
                .Distinct()
                .CountAsync();
            queue.TotalImages = totalGroups;
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "上传成功",
                imageId = image.Id,
                fileName = file.FileName
            });
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "上传图片时发生错误", details = ex.Message });
        }
    }

    [HttpGet("file")]
    public async Task<IActionResult> GetImageFile([FromQuery] string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BadRequest(new { message = "路径参数不能为空" });
        }

        // Validate path to prevent directory traversal
        if (path.Contains("..") || !path.StartsWith("/uploads/"))
        {
            return BadRequest(new { message = "无效的路径" });
        }

        var filePath = Path.Combine(_environment.ContentRootPath, path.TrimStart('/'));
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = "文件不存在" });
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var contentType = "image/jpeg"; // Default to JPEG, can be enhanced to detect actual type
        
        // Try to detect content type from file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return File(fileBytes, contentType);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteImage(int id)
    {
        var image = await _context.Images.FindAsync(id);
        if (image == null)
        {
            return NotFound(new { message = "图片不存在" });
        }

        // Delete file from disk
        var filePath = Path.Combine(_environment.ContentRootPath, image.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.Images.Remove(image);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

