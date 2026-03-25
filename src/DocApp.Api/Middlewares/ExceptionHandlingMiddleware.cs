using DocApp.Application.Common.Exceptions;
using DocApp.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace DocApp.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve => (400, "Validation Failed", ve.Errors),
            NotFoundException nfe => (404, nfe.Message, (IDictionary<string, string[]>?)null),
            SlotUnavailableException sue => (409, sue.Message, null),
            InvalidTimezoneException ite => (422, ite.Message, null),
            SkippedTimeException ste => (422, ste.Message, null),
            AmbiguousTimeException ate => (422, ate.Message, null),
            UnauthorizedAccessException => (401, "Unauthorized", null),
            _ => (500, "An unexpected error occurred.", null)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
            Errors = errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
