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

/// <summary>
/// 分页响应 DTO
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

