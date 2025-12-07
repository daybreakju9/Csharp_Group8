using Backend.DTOs;
using Backend.Helpers;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SelectionsController : ControllerBase
{
    private readonly ISelectionService _selectionService;

    public SelectionsController(ISelectionService selectionService)
    {
        _selectionService = selectionService;
    }

    [HttpPost]
    public async Task<ActionResult<SelectionDto>> CreateSelection([FromBody] CreateSelectionDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = this.GetUserId();
            var selection = await _selectionService.CreateAsync(createDto, userId);
            return CreatedAtAction(nameof(GetSelection), new { id = selection.Id }, selection);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SelectionDto>> GetSelection(int id)
    {
        var selection = await _selectionService.GetByIdAsync(id);
        if (selection == null)
        {
            return NotFound(new { message = "选择记录不存在" });
        }

        return Ok(selection);
    }

    [HttpGet("queue/{queueId}")]
    public async Task<ActionResult<List<SelectionDto>>> GetQueueSelections(int queueId, [FromQuery] int? userId = null)
    {
        var isAdmin = this.IsAdmin();
        int? targetUserId = userId;

        if (!userId.HasValue && !isAdmin)
        {
            // 非管理员用户只能查看自己的选择记录
            targetUserId = this.GetUserId();
        }

        var selections = await _selectionService.GetByQueueIdAsync(queueId, targetUserId, isAdmin);
        return Ok(selections);
    }

    [HttpGet("progress/{queueId}")]
    public async Task<ActionResult<UserProgressDto>> GetProgress(int queueId)
    {
        var userId = this.GetUserId();
        var progress = await _selectionService.GetProgressAsync(queueId, userId);
        return Ok(progress);
    }

    [HttpGet("progress/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserProgressDto>>> GetAllProgress([FromQuery] int? queueId = null)
    {
        var progressList = await _selectionService.GetAllProgressAsync(queueId);
        return Ok(progressList);
    }
}

