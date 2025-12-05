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
public class SelectionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SelectionsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<ActionResult<SelectionDto>> CreateSelection([FromBody] CreateSelectionDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();

        // Start a database transaction for data consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Check if user is a Guest (not allowed to participate in annotation)
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "用户不存在" });
            }

            if (user.Role == "Guest")
            {
                return Forbid();  // 403 Forbidden - Guest users cannot annotate
            }

            // Check if queue exists
            var queueExists = await _context.Queues.AnyAsync(q => q.Id == createDto.QueueId);
            if (!queueExists)
            {
                return BadRequest(new { message = "队列不存在" });
            }

            // Check if image exists
            var image = await _context.Images.FindAsync(createDto.SelectedImageId);
            if (image == null || image.QueueId != createDto.QueueId || image.ImageGroupId != createDto.ImageGroupId)
            {
                return BadRequest(new { message = "图片不存在或不属于指定队列" });
            }

            // Double-check if user has already selected this image group (prevent race condition)
            var existingSelection = await _context.SelectionRecords
                .FirstOrDefaultAsync(s => s.QueueId == createDto.QueueId
                                       && s.UserId == userId
                                       && s.ImageGroupId == createDto.ImageGroupId);

            if (existingSelection != null)
            {
                // Selection already exists, rollback transaction
                await transaction.RollbackAsync();
                return BadRequest(new { message = "您已经选择过这个图片组" });
            }

            // Create selection record
            var selection = new SelectionRecord
            {
                QueueId = createDto.QueueId,
                UserId = userId,
                ImageGroupId = createDto.ImageGroupId,
                SelectedImageId = createDto.SelectedImageId,
                DurationSeconds = createDto.DurationSeconds,
                CreatedAt = DateTime.UtcNow
            };

            _context.SelectionRecords.Add(selection);

            // Update or create user progress
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.QueueId == createDto.QueueId && p.UserId == userId);

            if (progress == null)
            {
                // Get total groups for this queue
                var totalGroups = await _context.ImageGroups
                    .Where(g => g.QueueId == createDto.QueueId)
                    .CountAsync();

                progress = new UserProgress
                {
                    QueueId = createDto.QueueId,
                    UserId = userId,
                    CompletedGroups = 1,
                    TotalGroups = totalGroups,
                    LastUpdated = DateTime.UtcNow
                };
                _context.UserProgresses.Add(progress);
            }
            else
            {
                progress.CompletedGroups++;
                progress.LastUpdated = DateTime.UtcNow;
            }

            // Save all changes within transaction
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            var selectionDto = await _context.SelectionRecords
                .Include(s => s.User)
                .Include(s => s.SelectedImage)
                .Include(s => s.ImageGroup)
                .Where(s => s.Id == selection.Id)
                .Select(s => new SelectionDto
                {
                    Id = s.Id,
                    QueueId = s.QueueId,
                    UserId = s.UserId,
                    Username = s.User.Username,
                    ImageGroupId = s.ImageGroupId,
                    ImageGroupName = s.ImageGroup.GroupName,
                    SelectedImageId = s.SelectedImageId,
                    SelectedImagePath = s.SelectedImage.FilePath,
                    SelectedFolderName = s.SelectedImage.FolderName,
                    DurationSeconds = s.DurationSeconds,
                    CreatedAt = s.CreatedAt
                })
                .FirstAsync();

            return CreatedAtAction(nameof(GetSelection), new { id = selection.Id }, selectionDto);
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "创建选择记录时发生错误", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SelectionDto>> GetSelection(int id)
    {
        var selection = await _context.SelectionRecords
            .Include(s => s.User)
            .Include(s => s.SelectedImage)
            .Include(s => s.ImageGroup)
            .Where(s => s.Id == id)
            .Select(s => new SelectionDto
            {
                Id = s.Id,
                QueueId = s.QueueId,
                UserId = s.UserId,
                Username = s.User.Username,
                ImageGroupId = s.ImageGroupId,
                ImageGroupName = s.ImageGroup.GroupName,
                SelectedImageId = s.SelectedImageId,
                SelectedImagePath = s.SelectedImage.FilePath,
                SelectedFolderName = s.SelectedImage.FolderName,
                DurationSeconds = s.DurationSeconds,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (selection == null)
        {
            return NotFound(new { message = "选择记录不存在" });
        }

        return Ok(selection);
    }

    [HttpGet("queue/{queueId}")]
    public async Task<ActionResult<List<SelectionDto>>> GetQueueSelections(int queueId, [FromQuery] int? userId = null)
    {
        var query = _context.SelectionRecords
            .Include(s => s.User)
            .Include(s => s.SelectedImage)
            .Include(s => s.ImageGroup)
            .Where(s => s.QueueId == queueId);

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }
        else if (!User.IsInRole("Admin"))
        {
            // Non-admin users can only see their own selections
            var currentUserId = GetUserId();
            query = query.Where(s => s.UserId == currentUserId);
        }

        var selections = await query
            .OrderBy(s => s.CreatedAt)
            .Select(s => new SelectionDto
            {
                Id = s.Id,
                QueueId = s.QueueId,
                UserId = s.UserId,
                Username = s.User.Username,
                ImageGroupId = s.ImageGroupId,
                ImageGroupName = s.ImageGroup.GroupName,
                SelectedImageId = s.SelectedImageId,
                SelectedImagePath = s.SelectedImage.FilePath,
                SelectedFolderName = s.SelectedImage.FolderName,
                DurationSeconds = s.DurationSeconds,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(selections);
    }

    [HttpGet("progress/{queueId}")]
    public async Task<ActionResult<UserProgressDto>> GetProgress(int queueId)
    {
        var userId = GetUserId();

        var progress = await _context.UserProgresses
            .Include(p => p.Queue)
            .Include(p => p.User)
            .Where(p => p.QueueId == queueId && p.UserId == userId)
            .Select(p => new UserProgressDto
            {
                QueueId = p.QueueId,
                QueueName = p.Queue.Name,
                UserId = p.UserId,
                Username = p.User.Username,
                CompletedGroups = p.CompletedGroups,
                TotalGroups = p.TotalGroups,
                ProgressPercentage = p.TotalGroups > 0 ? (decimal)p.CompletedGroups / p.TotalGroups * 100 : 0,
                LastUpdated = p.LastUpdated
            })
            .FirstOrDefaultAsync();

        if (progress == null)
        {
            // Create initial progress
            var totalGroups = await _context.ImageGroups
                .Where(g => g.QueueId == queueId)
                .CountAsync();

            var queue = await _context.Queues.FindAsync(queueId);
            var user = await _context.Users.FindAsync(userId);

            return Ok(new UserProgressDto
            {
                QueueId = queueId,
                QueueName = queue?.Name ?? "",
                UserId = userId,
                Username = user?.Username ?? "",
                CompletedGroups = 0,
                TotalGroups = totalGroups,
                ProgressPercentage = 0,
                LastUpdated = DateTime.UtcNow
            });
        }

        return Ok(progress);
    }

    [HttpGet("progress/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserProgressDto>>> GetAllProgress([FromQuery] int? queueId = null)
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
            .OrderByDescending(p => p.LastUpdated)
            .Select(p => new UserProgressDto
            {
                QueueId = p.QueueId,
                QueueName = p.Queue.Name,
                UserId = p.UserId,
                Username = p.User.Username,
                CompletedGroups = p.CompletedGroups,
                TotalGroups = p.TotalGroups,
                ProgressPercentage = p.TotalGroups > 0 ? (decimal)p.CompletedGroups / p.TotalGroups * 100 : 0,
                LastUpdated = p.LastUpdated
            })
            .ToListAsync();

        return Ok(progressList);
    }
}

