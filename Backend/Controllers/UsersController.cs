using Backend.DTOs;
using Backend.Helpers;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users with Guest role (pending approval)
    /// </summary>
    [HttpGet("guests")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetGuestUsers()
    {
        var guestUsers = await _userService.GetGuestUsersAsync();
        return Ok(guestUsers);
    }

    /// <summary>
    /// Get all users (for admin management)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
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

        try
        {
            var user = await _userService.ApproveUserAsync(approveDto.UserId);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reject/delete a guest user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new { message = "用户不存在" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

        try
        {
            var currentUserId = this.GetUserId();
            var user = await _userService.UpdateUserRoleAsync(id, updateDto.Role, currentUserId);
            return Ok(user);
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
}
