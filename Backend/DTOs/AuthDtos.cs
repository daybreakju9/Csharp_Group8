using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "用户名是必需的")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3到50个字符之间")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码是必需的")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须至少为6个字符")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "用户名是必需的")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码是必需的")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

