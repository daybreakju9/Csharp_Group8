using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Middleware;

/// <summary>
/// 捕获请求返回的 4xx/5xx 响应，输出包含关联 ID 的详细日志，并将日志落盘。
/// （不再处理异常，异常由 GlobalExceptionHandlerMiddleware 处理）
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly string _logDirectory;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // 对流式导出接口不包裹响应体，避免文件流被提前关闭
        if (context.Request.Path.StartsWithSegments("/api/export"))
        {
            var sw = Stopwatch.StartNew();
            
            // 导出接口直接调用下一个中间件，异常由全局处理器处理
            await _next(context);

            // 如果状态码是错误状态，记录日志
            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                var logEntry = BuildLogEntry(context, correlationId, sw.ElapsedMilliseconds, null, null);
                await WriteLogAsync(logEntry);
                _logger.LogWarning("导出请求错误: {Path} {StatusCode} CorrelationId={CorrelationId}",
                    context.Request.Path, context.Response.StatusCode, correlationId);
            }

            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // 直接调用下一个中间件，不再捕获异常
        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        // 记录所有错误响应（4xx, 5xx）
        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            var logEntry = BuildLogEntry(
                context,
                correlationId,
                elapsedMs,
                Truncate(responseText, 4000),
                null); // 不再记录异常，因为已经由全局处理器处理

            await WriteLogAsync(logEntry);

            // 根据状态码级别记录不同日志
            if (context.Response.StatusCode >= 500)
            {
                _logger.LogError(
                    "服务器错误: {Method} {Path} 状态码={StatusCode}, 耗时={ElapsedMs}ms, CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs,
                    correlationId);
            }
            else
            {
                _logger.LogWarning(
                    "客户端错误: {Method} {Path} 状态码={StatusCode}, 耗时={ElapsedMs}ms, CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs,
                    correlationId);
            }
        }

        // 记录慢请求（超过1秒）
        if (elapsedMs > 1000)
        {
            _logger.LogWarning("慢请求: {Method} {Path} - {ElapsedMs}ms CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                correlationId);
        }

        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }

    private string BuildLogEntry(
        HttpContext context,
        string correlationId,
        long elapsedMs,
        string? responseBody,
        Exception? exception)
    {
        var request = context.Request;
        var statusCode = context.Response.StatusCode;
        var user = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity!.Name
            : "anonymous";

        var logBuilder = new StringBuilder();
        logBuilder.AppendLine($"[{DateTime.UtcNow:O}] CorrelationId={correlationId}");
        logBuilder.AppendLine($"Status={statusCode}, DurationMs={elapsedMs}");
        logBuilder.AppendLine($"Request: {request.Method} {request.Path}{request.QueryString}");
        logBuilder.AppendLine($"User={user}, RemoteIP={context.Connection.RemoteIpAddress}");
        logBuilder.AppendLine($"ContentType={request.ContentType}, ContentLength={request.ContentLength ?? 0}, UserAgent={request.Headers["User-Agent"]}");

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            logBuilder.AppendLine($"ResponseBody={responseBody}");
        }

        if (exception != null)
        {
            logBuilder.AppendLine($"Exception={exception}");
        }

        logBuilder.AppendLine(new string('-', 80));

        return logBuilder.ToString();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(0, maxLength) + "...(truncated)";
    }

    private async Task WriteLogAsync(string content)
    {
        var logFile = Path.Combine(_logDirectory, $"backend-{DateTime.UtcNow:yyyyMMdd}.log");
        await File.AppendAllTextAsync(logFile, content, Encoding.UTF8);
    }
}
