using Api.Http;
using Domain.Exceptions;
using FluentValidation;

namespace Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors
                .Select(e => new ApiErrorDetail(e.PropertyName, e.ErrorMessage))
                .ToList();
            await WriteError(context, StatusCodes.Status400BadRequest, "VALIDATION_ERROR",
                "Erro de validação.", details);
        }
        catch (NotFoundException ex)
        {
            await WriteError(context, StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message);
        }
        catch (InvalidEntityException ex)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "INVALID_PARAMETERS", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR",
                "Erro interno no servidor.");
        }
    }

    private static async Task WriteError(
        HttpContext context,
        int status,
        string code,
        string message,
        IReadOnlyList<ApiErrorDetail>? details = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = status;

        var payload = new ErrorResponse(false, new ApiError(code, message, details));
        await context.Response.WriteAsJsonAsync(payload, ApiJson.Options);
    }
}
