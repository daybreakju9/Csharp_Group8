using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueuesController : ControllerBase
{
    private readonly AppDbContext _context;

    public QueuesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<QueueDto>>> GetQueues([FromQuery] int? projectId = null)
    {
        var query = _context.Queues
            .Include(q => q.Project)
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(q => q.ProjectId == projectId.Value);
        }

        var queues = await query
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QueueDto
            {
                Id = q.Id,
                ProjectId = q.ProjectId,
                ProjectName = q.Project.Name,
                Name = q.Name,
                Description = q.Description,
                ComparisonCount = q.ComparisonCount,
                GroupCount = q.GroupCount,
                TotalImageCount = q.TotalImageCount,
                Status = q.Status,
                IsRandomOrder = q.IsRandomOrder,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .ToListAsync();

        return Ok(queues);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QueueDto>> GetQueue(int id)
    {
        var queue = await _context.Queues
            .Include(q => q.Project)
            .Where(q => q.Id == id)
            .Select(q => new QueueDto
            {
                Id = q.Id,
                ProjectId = q.ProjectId,
                ProjectName = q.Project.Name,
                Name = q.Name,
                Description = q.Description,
                ComparisonCount = q.ComparisonCount,
                GroupCount = q.GroupCount,
                TotalImageCount = q.TotalImageCount,
                Status = q.Status,
                IsRandomOrder = q.IsRandomOrder,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (queue == null)
        {
            return NotFound(new { message = "队列不存在" });
        }

        return Ok(queue);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QueueDto>> CreateQueue([FromBody] CreateQueueDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if project exists
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == createDto.ProjectId);
        if (!projectExists)
        {
            return BadRequest(new { message = "项目不存在" });
        }

        var queue = new Queue
        {
            ProjectId = createDto.ProjectId,
            Name = createDto.Name,
            Description = createDto.Description,
            ComparisonCount = createDto.ComparisonCount,
            GroupCount = 0,
            TotalImageCount = 0,
            Status = QueueStatus.Draft,
            IsRandomOrder = createDto.IsRandomOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Queues.Add(queue);
        await _context.SaveChangesAsync();

        var queueDto = await _context.Queues
            .Include(q => q.Project)
            .Where(q => q.Id == queue.Id)
            .Select(q => new QueueDto
            {
                Id = q.Id,
                ProjectId = q.ProjectId,
                ProjectName = q.Project.Name,
                Name = q.Name,
                Description = q.Description,
                ComparisonCount = q.ComparisonCount,
                GroupCount = q.GroupCount,
                TotalImageCount = q.TotalImageCount,
                Status = q.Status,
                IsRandomOrder = q.IsRandomOrder,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetQueue), new { id = queue.Id }, queueDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QueueDto>> UpdateQueue(int id, [FromBody] UpdateQueueDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var queue = await _context.Queues.FindAsync(id);
        if (queue == null)
        {
            return NotFound(new { message = "队列不存在" });
        }

        queue.Name = updateDto.Name;
        queue.Description = updateDto.Description;
        queue.ComparisonCount = updateDto.ComparisonCount;

        if (!string.IsNullOrEmpty(updateDto.Status))
        {
            queue.Status = updateDto.Status;
        }

        if (updateDto.IsRandomOrder.HasValue)
        {
            queue.IsRandomOrder = updateDto.IsRandomOrder.Value;
        }

        queue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var queueDto = await _context.Queues
            .Include(q => q.Project)
            .Where(q => q.Id == id)
            .Select(q => new QueueDto
            {
                Id = q.Id,
                ProjectId = q.ProjectId,
                ProjectName = q.Project.Name,
                Name = q.Name,
                Description = q.Description,
                ComparisonCount = q.ComparisonCount,
                GroupCount = q.GroupCount,
                TotalImageCount = q.TotalImageCount,
                Status = q.Status,
                IsRandomOrder = q.IsRandomOrder,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .FirstAsync();

        return Ok(queueDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteQueue(int id)
    {
        var queue = await _context.Queues.FindAsync(id);
        if (queue == null)
        {
            return NotFound(new { message = "队列不存在" });
        }

        _context.Queues.Remove(queue);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

