using Backend.Data;
using Backend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExportController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("selections")]
    public async Task<IActionResult> ExportSelections([FromQuery] int queueId, [FromQuery] string format = "csv")
    {
        var queue = await _context.Queues
            .FirstOrDefaultAsync(q => q.Id == queueId);

        if (queue == null)
        {
            return NotFound(new { message = "队列不存在" });
        }

        if (format.ToLower() == "csv")
        {
            var fileName = $"selections_{queue.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // Use streaming for large datasets
            return new FileCallbackResult("text/csv", fileName, async (outputStream, _) =>
            {
                await using var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true);

                // Write CSV header
                await writer.WriteLineAsync("用户ID,用户名,图片组,选择的文件夹,选择的文件名,选择时间");

                // Stream data from database in batches
                var pageSize = 1000;
                var pageNumber = 0;
                bool hasMoreData = true;

                while (hasMoreData)
                {
                    var selections = await _context.SelectionRecords
                        .IgnoreQueryFilters() // 包含已软删的图片，确保导出完整
                        .Include(s => s.User)
                        .Include(s => s.SelectedImage)
                        .Where(s => s.QueueId == queueId)
                        .OrderBy(s => s.ImageGroupId)
                        .ThenBy(s => s.UserId)
                        .Skip(pageNumber * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (selections.Count == 0)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        foreach (var selection in selections)
                        {
                            var selectedImage = selection.SelectedImage;
                            var folder = selectedImage?.FolderName ?? "";
                            var fileNameOnly = selectedImage?.FileName ?? "";
                            await writer.WriteLineAsync($"{selection.UserId},{selection.User.Username},{selection.ImageGroupId},{folder},{fileNameOnly},{selection.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                        }
                        pageNumber++;
                        hasMoreData = selections.Count == pageSize;
                    }
                }

                await writer.FlushAsync();
            });
        }
        else if (format.ToLower() == "json")
        {
            var fileName = $"selections_{queue.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            // Use streaming for JSON export as well
            return new FileCallbackResult("application/json", fileName, async (outputStream, _) =>
            {
                await using var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true);

                await writer.WriteAsync("[");

                var pageSize = 1000;
                var pageNumber = 0;
                bool hasMoreData = true;
                bool isFirst = true;

                while (hasMoreData)
                {
                    var selections = await _context.SelectionRecords
                        .IgnoreQueryFilters()
                        .Include(s => s.User)
                        .Include(s => s.SelectedImage)
                        .Where(s => s.QueueId == queueId)
                        .OrderBy(s => s.ImageGroupId)
                        .ThenBy(s => s.UserId)
                        .Skip(pageNumber * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (selections.Count == 0)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        foreach (var selection in selections)
                        {
                            if (!isFirst)
                            {
                                await writer.WriteAsync(",");
                            }
                            isFirst = false;

                            var json = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                UserId = selection.UserId,
                                Username = selection.User.Username,
                                ImageGroup = selection.ImageGroupId,
                                SelectedFolderName = selection.SelectedImage?.FolderName,
                                SelectedFileName = selection.SelectedImage?.FileName,
                                SelectedFilePath = selection.SelectedImage?.FilePath,
                                CreatedAt = selection.CreatedAt
                            });
                            await writer.WriteAsync(json);
                        }
                        pageNumber++;
                        hasMoreData = selections.Count == pageSize;
                    }
                }

                await writer.WriteAsync("]");
                await writer.FlushAsync();
            });
        }
        else
        {
            return BadRequest(new { message = "不支持的导出格式。支持的格式：csv, json" });
        }
    }

    [HttpGet("progress")]
    public async Task<IActionResult> ExportProgress([FromQuery] int? queueId = null, [FromQuery] string format = "csv")
    {
        if (format.ToLower() == "csv")
        {
            var fileName = $"progress_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            // Use streaming for large datasets
            return new FileCallbackResult("text/csv", fileName, async (outputStream, _) =>
            {
                await using var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true);

                // Write CSV header
                await writer.WriteLineAsync("队列ID,队列名称,用户ID,用户名,已完成,总计,进度百分比,最后更新");

                // Stream data from database in batches
                var pageSize = 1000;
                var pageNumber = 0;
                bool hasMoreData = true;

                while (hasMoreData)
                {
                    var query = _context.UserProgresses
                        .Include(p => p.Queue)
                        .Include(p => p.User)
                        .AsQueryable();

                    if (queueId.HasValue)
                    {
                        query = query.Where(p => p.QueueId == queueId.Value);
                    }

                    var progressList = await query
                        .OrderBy(p => p.QueueId)
                        .ThenBy(p => p.UserId)
                        .Skip(pageNumber * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (progressList.Count == 0)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        foreach (var progress in progressList)
                        {
                            var percentage = progress.TotalGroups > 0 ? (double)progress.CompletedGroups / progress.TotalGroups * 100 : 0;
                            await writer.WriteLineAsync($"{progress.QueueId},{progress.Queue.Name},{progress.UserId},{progress.User.Username},{progress.CompletedGroups},{progress.TotalGroups},{percentage:F2}%,{progress.LastUpdated:yyyy-MM-dd HH:mm:ss}");
                        }
                        pageNumber++;
                        hasMoreData = progressList.Count == pageSize;
                    }
                }

                await writer.FlushAsync();
            });
        }
        else if (format.ToLower() == "json")
        {
            var fileName = $"progress_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            // Use streaming for JSON export
            return new FileCallbackResult("application/json", fileName, async (outputStream, _) =>
            {
                await using var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true);

                await writer.WriteAsync("[");

                var pageSize = 1000;
                var pageNumber = 0;
                bool hasMoreData = true;
                bool isFirst = true;

                while (hasMoreData)
                {
                    var query = _context.UserProgresses
                        .Include(p => p.Queue)
                        .Include(p => p.User)
                        .AsQueryable();

                    if (queueId.HasValue)
                    {
                        query = query.Where(p => p.QueueId == queueId.Value);
                    }

                    var progressList = await query
                        .OrderBy(p => p.QueueId)
                        .ThenBy(p => p.UserId)
                        .Skip(pageNumber * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (progressList.Count == 0)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        foreach (var progress in progressList)
                        {
                            if (!isFirst)
                            {
                                await writer.WriteAsync(",");
                            }
                            isFirst = false;

                            var json = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                QueueId = progress.QueueId,
                                QueueName = progress.Queue.Name,
                                UserId = progress.UserId,
                                Username = progress.User.Username,
                                CompletedGroups = progress.CompletedGroups,
                                TotalGroups = progress.TotalGroups,
                                ProgressPercentage = progress.TotalGroups > 0 ? (double)progress.CompletedGroups / progress.TotalGroups * 100 : 0,
                                LastUpdated = progress.LastUpdated
                            });
                            await writer.WriteAsync(json);
                        }
                        pageNumber++;
                        hasMoreData = progressList.Count == pageSize;
                    }
                }

                await writer.WriteAsync("]");
                await writer.FlushAsync();
            });
        }
        else
        {
            return BadRequest(new { message = "不支持的导出格式。支持的格式：csv, json" });
        }
    }
}

