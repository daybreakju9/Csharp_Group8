using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Helpers;

/// <summary>
/// 控制器扩展方法
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// 从JWT Token中获取当前用户ID
    /// </summary>
    /// <param name="controller">控制器实例</param>
    /// <returns>用户ID</returns>
    /// <exception cref="UnauthorizedAccessException">当用户ID未找到时抛出</exception>
    public static int GetUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("用户ID未找到，请重新登录");
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("无效的用户ID格式");
        }

        return userId;
    }

    /// <summary>
    /// 检查当前用户是否为管理员
    /// </summary>
    /// <param name="controller">控制器实例</param>
    /// <returns>如果是管理员返回true，否则返回false</returns>
    public static bool IsAdmin(this ControllerBase controller)
    {
        return controller.User.IsInRole("Admin");
    }

    /// <summary>
    /// 检查当前用户是否为Guest
    /// </summary>
    /// <param name="controller">控制器实例</param>
    /// <returns>如果是Guest返回true，否则返回false</returns>
    public static bool IsGuest(this ControllerBase controller)
    {
        return controller.User.IsInRole("Guest");
    }
}

