using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Middleware;

/// <summary>
/// 捕获请求返回的 4xx/5xx 响应以及未处理异常，输出包含关联 ID 的详细日志，并将日志落盘。
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
            Exception? exportException = null;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exportException = ex;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex, "Export request failed: {Path} CorrelationId={CorrelationId}", context.Request.Path, correlationId);
            }

            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
            {
                var logEntry = BuildLogEntry(context, correlationId, sw.ElapsedMilliseconds, null, exportException);
                await WriteLogAsync(logEntry);
            }

            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            responseBody.SetLength(0);
            var payload = JsonSerializer.Serialize(new
            {
                message = "服务器内部错误，请稍后再试",
                correlationId
            });
            var buffer = Encoding.UTF8.GetBytes(payload);
            await responseBody.WriteAsync(buffer, 0, buffer.Length);
            responseBody.Seek(0, SeekOrigin.Begin);
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            var logEntry = BuildLogEntry(
                context,
                correlationId,
                elapsedMs,
                Truncate(responseText, 4000),
                capturedException);

            await WriteLogAsync(logEntry);

            if (capturedException != null)
            {
                _logger.LogError(capturedException,
                    "请求异常: {Method} {Path} 状态码={StatusCode}, CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    correlationId);
            }
            else
            {
                _logger.LogWarning(
                    "请求返回错误: {Method} {Path} 状态码={StatusCode}, CorrelationId={CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    correlationId);
            }
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

