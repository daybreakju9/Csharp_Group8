namespace Backend.DTOs;

/// <summary>
/// 统一的API错误响应格式
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? Details { get; set; }
}

/// <summary>
/// 统一的API成功响应格式
/// </summary>
public class SuccessResponse
{
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// 统一的API成功响应格式（泛型版本）
/// </summary>
public class SuccessResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
