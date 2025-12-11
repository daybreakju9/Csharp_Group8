using Microsoft.AspNetCore.Mvc;

namespace Backend.Helpers;

/// <summary>
/// Custom ActionResult for streaming file downloads
/// </summary>
public class FileCallbackResult : FileResult
{
    private readonly Func<Stream, ActionContext, Task> _callback;

    public FileCallbackResult(string contentType, string? fileDownloadName, Func<Stream, ActionContext, Task> callback)
        : base(contentType)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        FileDownloadName = fileDownloadName;
    }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var executor = new FileCallbackResultExecutor(context.HttpContext.Response, ContentType, FileDownloadName, _callback);
        return executor.ExecuteAsync(context);
    }

    private class FileCallbackResultExecutor
    {
        private readonly HttpResponse _response;
        private readonly string _contentType;
        private readonly string? _fileDownloadName;
        private readonly Func<Stream, ActionContext, Task> _callback;

        public FileCallbackResultExecutor(HttpResponse response, string contentType, string? fileDownloadName, Func<Stream, ActionContext, Task> callback)
        {
            _response = response;
            _contentType = contentType;
            _fileDownloadName = fileDownloadName;
            _callback = callback;
        }

        public async Task ExecuteAsync(ActionContext context)
        {
            _response.ContentType = _contentType;

            if (!string.IsNullOrEmpty(_fileDownloadName))
            {
                var encodedFileName = System.Web.HttpUtility.UrlEncode(_fileDownloadName, System.Text.Encoding.UTF8);
                _response.Headers.Append("Content-Disposition", $"attachment; filename=\"{encodedFileName}\"");
            }

            await _callback(_response.Body, context);
            await _response.Body.FlushAsync();
        }
    }
}
