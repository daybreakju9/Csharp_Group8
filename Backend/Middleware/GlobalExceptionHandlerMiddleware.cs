using System.Net;
using System.Text.Json;

namespace Backend.Middleware;

/// <summary>
/// 全局异常处理器，捕获所有未处理的异常并返回统一的错误响应
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // 设置默认值
        var response = context.Response;
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "服务器内部错误";
        object? errorDetails = null;

        // 根据异常类型设置不同的状态码和消息
        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "未授权访问";
                break;

            case ArgumentException:
            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case KeyNotFoundException:
            case FileNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = "请求的资源不存在";
                break;

            case NotImplementedException:
                statusCode = (int)HttpStatusCode.NotImplemented;
                message = "功能尚未实现";
                break;

            case TimeoutException:
                statusCode = (int)HttpStatusCode.RequestTimeout;
                message = "请求超时";
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "服务器内部错误";
                // 开发环境显示详细错误
                if (_env.IsDevelopment())
                {
                    errorDetails = new
                    {
                        exception.Message,
                        exception.StackTrace,
                        exceptionType = exception.GetType().FullName
                    };
                }
                break;
        }

        response.StatusCode = statusCode;

        // 构建响应对象
        var errorResponse = new
        {
            success = false,
            message,
            requestId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow,
            details = errorDetails
        };

        // 记录错误日志
        _logger.LogError(exception, 
            "全局异常捕获: {Method} {Path} - {StatusCode} CorrelationId={RequestId}",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            context.TraceIdentifier);

        // 返回JSON响应
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        
        // 确保响应流仍然可用
        if (!response.HasStarted)
        {
            await response.WriteAsync(jsonResponse);
        }
        else
        {
            // 如果响应已经开始，尝试使用替代方式记录错误
            _logger.LogWarning("无法写入响应流，因为响应已经开始。错误信息: {Message}", exception.Message);
        }
    }
}
