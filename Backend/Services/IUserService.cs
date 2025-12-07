using Backend.DTOs;

namespace Backend.Services;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取所有用户
    /// </summary>
    Task<IEnumerable<UserDto>> GetAllAsync();

    /// <summary>
    /// 获取所有游客用户
    /// </summary>
    Task<IEnumerable<UserDto>> GetGuestUsersAsync();

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    Task<UserDto?> GetByIdAsync(int id);

    /// <summary>
    /// 批准游客用户，升级为普通用户
    /// </summary>
    Task<UserDto> ApproveUserAsync(int userId);

    /// <summary>
    /// 更新用户角色
    /// </summary>
    Task<UserDto> UpdateUserRoleAsync(int userId, string role, int currentUserId);

    /// <summary>
    /// 删除用户
    /// </summary>
    Task<bool> DeleteUserAsync(int userId);
}

