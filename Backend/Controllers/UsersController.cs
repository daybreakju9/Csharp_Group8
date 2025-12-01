using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all users with Guest role (pending approval)
    /// </summary>
    [HttpGet("guests")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetGuestUsers()
    {
        var guestUsers = await _context.Users
            .Where(u => u.Role == "Guest")
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(guestUsers);
    }

    /// <summary>
    /// Get all users (for admin management)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Approve a guest user and upgrade to User role
    /// </summary>
    [HttpPost("approve")]
    public async Task<ActionResult<UserDto>> ApproveUser([FromBody] ApproveUserDto approveDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(approveDto.UserId);

        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        if (user.Role != "Guest")
        {
            return BadRequest(new { message = "只能批准游客账号" });
        }

        // Upgrade user to regular User role
        user.Role = "User";
        await _context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Reject/delete a guest user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        if (user.Role == "Admin")
        {
            return BadRequest(new { message = "不能删除管理员账号" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Update user role (Admin can change any user's role)
    /// </summary>
    [HttpPut("{id}/role")]
    public async Task<ActionResult<UserDto>> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        // Prevent changing own role (admin shouldn't be able to demote themselves)
        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
        if (user.Id == currentUserId)
        {
            return BadRequest(new { message = "不能修改自己的角色" });
        }

        user.Role = updateDto.Role;
        await _context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }
}
