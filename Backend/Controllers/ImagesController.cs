using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IImageGroupService _imageGroupService;
    private readonly IFileStorageService _fileStorage;

    public ImagesController(
        IImageService imageService,
        IImageGroupService imageGroupService,
        IFileStorageService fileStorage)
    {
        _imageService = imageService;
        _imageGroupService = imageGroupService;
        _fileStorage = fileStorage;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    /// <summary>
    /// 获取队列的所有图片（分页）
    /// </summary>
    [HttpGet("queue/{queueId}")]
    public async Task<ActionResult<PagedResult<ImageDto>>> GetQueueImages(
        int queueId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? groupId = null)
    {
        var (items, totalCount) = await _imageService.GetPagedAsync(
            queueId, pageNumber, pageSize, searchTerm, groupId);

        var result = new PagedResult<ImageDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(result);
    }

    /// <summary>
    /// 获取队列的所有图片组（分页）
    /// </summary>
    [HttpGet("groups/{queueId}")]
    public async Task<ActionResult<PagedResult<ImageGroupDto>>> GetImageGroups(
        int queueId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null)
    {
        var (items, totalCount) = await _imageGroupService.GetPagedAsync(
            queueId, pageNumber, pageSize, searchTerm);

        var result = new PagedResult<ImageGroupDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(result);
    }

    /// <summary>
    /// 获取下一个未标注的图片组
    /// </summary>
    [HttpGet("next-group/{queueId}")]
    public async Task<ActionResult<ImageGroupDto>> GetNextImageGroup(int queueId)
    {
        var userId = GetUserId();

        var nextGroup = await _imageService.GetNextUnannotatedGroupAsync(queueId, userId);

        if (nextGroup == null)
        {
            return Ok(new { message = "所有图片组已完成选择", completed = true });
        }

        return Ok(nextGroup);
    }

    /// <summary>
    /// 上传单个图片
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ImageDto>> UploadImage(
        [FromForm] int queueId,
        [FromForm] string folderName,
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "没有上传文件" });
        }

        if (string.IsNullOrWhiteSpace(folderName))
        {
            return BadRequest(new { message = "文件夹名称不能为空" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var (imageDto, isDuplicate) = await _imageService.UploadAsync(queueId, folderName, file.FileName, stream);

            return Ok(new
            {
                message = isDuplicate ? "图片已存在，已跳过" : "上传成功",
                data = imageDto,
                isDuplicate
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "上传图片时发生错误", details = ex.Message });
        }
    }

    /// <summary>
    /// 批量上传图片
    /// </summary>
    [HttpPost("upload-batch")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UploadBatch(
        [FromForm] int queueId,
        [FromForm] List<IFormFile> files,
        [FromForm] List<string> folderNames)
    {
        if (files == null || !files.Any())
        {
            return BadRequest(new { message = "没有上传文件" });
        }

        if (files.Count != folderNames.Count)
        {
            return BadRequest(new { message = "文件和文件夹名称数量不匹配" });
        }

        try
        {
            // 按文件夹组织文件
            var folderFiles = new Dictionary<string, List<(string fileName, Stream fileStream)>>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var folderName = folderNames[i];

                if (!folderFiles.ContainsKey(folderName))
                {
                    folderFiles[folderName] = new List<(string, Stream)>();
                }

                folderFiles[folderName].Add((file.FileName, file.OpenReadStream()));
            }

            var result = await _imageService.UploadBatchAsync(queueId, folderFiles);

            // 关闭所有流
            foreach (var kvp in folderFiles)
            {
                foreach (var (_, stream) in kvp.Value)
                {
                    stream.Dispose();
                }
            }

            return Ok(new
            {
                message = $"批量上传完成",
                successCount = result.SuccessCount,
                skippedCount = result.SkippedCount,
                failureCount = result.FailureCount,
                totalGroups = result.TotalGroups,
                errors = result.Errors,
                skippedFiles = result.SkippedFiles
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批量上传时发生错误", details = ex.Message });
        }
    }

    /// <summary>
    /// 获取图片文件
    /// </summary>
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

        var fileBytes = await _fileStorage.GetFileAsync(path);

        if (fileBytes == null)
        {
            return NotFound(new { message = "文件不存在" });
        }

        // Detect content type from file extension
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var contentType = extension switch
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

    /// <summary>
    /// 删除单个图片
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteImage(int id)
    {
        try
        {
            var success = await _imageService.DeleteAsync(id);

            if (!success)
            {
                return NotFound(new { message = "图片不存在" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除图片时发生错误", details = ex.Message });
        }
    }

    /// <summary>
    /// 批量删除图片
    /// </summary>
    [HttpPost("delete-batch")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteBatch([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest(new { message = "没有提供要删除的图片ID" });
        }

        try
        {
            var count = await _imageService.DeleteBatchAsync(ids);

            return Ok(new { message = $"成功删除 {count} 张图片", deletedCount = count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批量删除时发生错误", details = ex.Message });
        }
    }
}
