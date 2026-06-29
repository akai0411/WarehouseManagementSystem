using FluentValidation;
using System.Net;
using System.Text.Json;
using WarehouseManagementSystem.Application.Common.Exceptions;
/// <summary>
/// Middleware that handles unhandled exceptions and returns a standardized error response.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">Logger used to record exceptions.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    /// <summary>
    /// Processes the current HTTP request and handles any unhandled exceptions.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleKnownException(
                context,
                ex,
                HttpStatusCode.BadRequest,
                ex.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            );
        }
        catch (ConflictException ex)
        {
            await HandleKnownException(context, ex, HttpStatusCode.Conflict);
        }
        catch (NotFoundException ex)
        {
            await HandleKnownException(context, ex, HttpStatusCode.NotFound);
        }
        catch (UnauthorizedException ex)
        {
            await HandleKnownException(context, ex, HttpStatusCode.Unauthorized);
        }
        catch (BusinessRuleException ex)
        {
            await HandleKnownException(context, ex, HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception ex)
        {
            await HandleUnknownException(context, ex);
        }
    }

    private async Task HandleKnownException(HttpContext context, Exception ex, HttpStatusCode statusCode, object? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var traceId = context.TraceIdentifier;

        _logger.LogWarning(ex,
            "Handled exception. TraceId: {TraceId}, Path: {Path}",
            traceId,
            context.Request.Path);

        var response = new
        {
            success = false,
            message = ex is ValidationException
            ? "Validation failed."
            : ex.Message,
            errors,
            traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private async Task HandleUnknownException(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var traceId = context.TraceIdentifier;

        _logger.LogError(ex,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}",
            traceId,
            context.Request.Path);

        var response = new
        {
            success = false,
            message = "An unexpected error occurred.",
            traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}