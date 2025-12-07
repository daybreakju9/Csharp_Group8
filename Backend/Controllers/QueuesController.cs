using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueuesController : ControllerBase
{
    private readonly IQueueService _queueService;

    public QueuesController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QueueDto>>> GetQueues([FromQuery] int? projectId = null)
    {
        var queues = await _queueService.GetAllAsync(projectId);
        return Ok(queues);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QueueDto>> GetQueue(int id)
    {
        var queue = await _queueService.GetByIdAsync(id);
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

        try
        {
            var queue = await _queueService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetQueue), new { id = queue.Id }, queue);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QueueDto>> UpdateQueue(int id, [FromBody] UpdateQueueDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var queue = await _queueService.UpdateAsync(id, updateDto);
        if (queue == null)
        {
            return NotFound(new { message = "队列不存在" });
        }

        return Ok(queue);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteQueue(int id)
    {
        var success = await _queueService.DeleteAsync(id);
        if (!success)
        {
            return NotFound(new { message = "队列不存在" });
        }

        return NoContent();
    }
}

