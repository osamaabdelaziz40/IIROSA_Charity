using System.Net;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace IIROSA.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and returns structured error responses
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
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
            _logger.LogError(ex, "Unhandled exception occurred. Request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = exception.Message,
            Status = HttpStatusCode.InternalServerError,
            TraceId = context.TraceIdentifier
        };

        // Include stack trace in development
        if (_env.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
            response.InnerException = exception.InnerException?.Message;
            response.Details = new Dictionary<string, string>
            {
                { "ExceptionType", exception.GetType().Name },
                { "Source", exception.Source ?? string.Empty }
            };
        }

        // Handle specific exception types
        switch (exception)
        {
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Status = HttpStatusCode.Unauthorized;
                response.Message = "You are not authorized to access this resource";
                break;

            // A failed validator is a bad request, and the client needs to know WHICH field failed
            // so it can flag it — a message string alone cannot do that. Errors are grouped by
            // property name because one field can break several rules at once.
            case FluentValidation.ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Status = HttpStatusCode.BadRequest;
                response.Message = "One or more fields are invalid";
                response.Errors = validationException.Errors
                    .GroupBy(error => error.PropertyName ?? string.Empty)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray());
                break;

            // The caller is authenticated and holds the right role; the refusal is about the
            // charity's own lock/permission state, so it is a 403 rather than a 401 or a 404.
            case IIROSA.Application.Interfaces.CharityWriteForbiddenException:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Status = HttpStatusCode.Forbidden;
                break;

            case KeyNotFoundException:
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Status = HttpStatusCode.NotFound;
                break;

            case ArgumentException:
            case BadHttpRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Status = HttpStatusCode.BadRequest;
                break;

            // Architecture §5.1 — the domain's own faults carry their contract statuses, not a
            // blanket 500: a missing record answers 404 and a broken business rule answers 400.
            // Both types inherit Exception directly, so before these cases existed they fell
            // through to the default and every domain refusal surfaced as an internal error.
            // The domain message IS the product copy here (bilingual-message sweep is a
            // recorded deferral) — it ships in every environment.
            case IIROSA.Application.Exceptions.NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Status = HttpStatusCode.NotFound;
                break;

            case IIROSA.Application.Exceptions.BusinessException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Status = HttpStatusCode.BadRequest;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // AC 2 — raw exception text never ships outside development. "error.unexpected"
                // is the stable key the SPA's /error page localises; NLog keeps the full
                // exception server-side either way (AC 3 — the pre-switch LogError above).
                if (!_env.IsDevelopment())
                {
                    response.Message = "error.unexpected";
                }
                break;
        }

        // Log additional details
        _logger.LogError("Exception Details: {ExceptionType} | Message: {Message} | Path: {Path}",
            exception.GetType().Name,
            exception.Message,
            context.Request.Path);

        // Check if user is authenticated
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            _logger.LogError("Authenticated User: {UserName} | Email: {Email} | Roles: {Roles}",
                user.Identity.Name,
                user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                string.Join(", ", user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Standard error response structure
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public HttpStatusCode Status { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public Dictionary<string, string>? Details { get; set; }

    /// <summary>
    /// Per-field validation errors, keyed by property name. Present only on a 400 raised by a
    /// validator; the client uses the keys to flag the offending fields on the form.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Extension method to register the exception middleware
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}
