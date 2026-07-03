using System.Text.Json;
using Api.Http;

namespace Api.Middlewares;

public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrapperMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        buffer.Seek(0, SeekOrigin.Begin);

        if (ShouldWrap(context, buffer))
        {
            using var doc = await JsonDocument.ParseAsync(buffer);
            var payload = JsonSerializer.SerializeToUtf8Bytes(
                new { success = true, data = doc.RootElement }, ApiJson.Options);

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength = payload.Length;
            await originalBody.WriteAsync(payload);
        }
        else
        {
            context.Response.ContentLength = buffer.Length;
            await buffer.CopyToAsync(originalBody);
        }
    }

    private static bool ShouldWrap(HttpContext context, Stream buffer)
    {
        var status = context.Response.StatusCode;
        var isJson = context.Response.ContentType?
            .Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;

        return status is >= 200 and < 300 && isJson && buffer.Length > 0;
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path;
        return path.StartsWithSegments("/openapi") || path.StartsWithSegments("/swagger");
    }
}
