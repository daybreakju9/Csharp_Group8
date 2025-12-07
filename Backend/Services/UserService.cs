using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return users.OrderBy(u => u.CreatedAt).Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<IEnumerable<UserDto>> GetGuestUsersAsync()
    {
        var guestUsers = await _unitOfWork.Users.FindAsync(u => u.Role == "Guest");
        return guestUsers.OrderBy(u => u.CreatedAt).Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> ApproveUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("用户不存在");
        }

        if (user.Role != "Guest")
        {
            throw new InvalidOperationException("只能批准游客账号");
        }

        // 升级用户为普通用户
        user.Role = "User";
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> UpdateUserRoleAsync(int userId, string role, int currentUserId)
    {
        // 验证角色值
        if (role != "Guest" && role != "User" && role != "Admin")
        {
            throw new ArgumentException("角色必须是 Guest、User 或 Admin");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("用户不存在");
        }

        // 防止管理员修改自己的角色
        if (user.Id == currentUserId)
        {
            throw new InvalidOperationException("不能修改自己的角色");
        }

        // 更新角色
        user.Role = role;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // 不能删除管理员账号
        if (user.Role == "Admin")
        {
            throw new InvalidOperationException("不能删除管理员账号");
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

