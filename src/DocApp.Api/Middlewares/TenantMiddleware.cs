using DocApp.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DocApp.Api.Middlewares;

/// <summary>
/// Resolves and sets the tenant context for every request.
/// Must run BEFORE authorization middleware.
/// </summary>
public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        TenantResolutionService resolver,
        TenantContextAccessor tenantContextAccessor)
    {
        var tenant = await resolver.ResolveAsync(context);

        if (tenant is null)
        {
            // Allow health-check and auth endpoints without a tenant header
            var path = context.Request.Path.Value ?? "";
            if (path.StartsWith("/health") || path.StartsWith("/api/auth"))
            {
                await _next(context);
                return;
            }

            _logger.LogWarning("Request rejected: no valid tenant resolved for path {Path}", context.Request.Path);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Tenant Required",
                Status = 400,
                Detail = "A valid X-Tenant-ID header or tenant subdomain is required."
            });
            return;
        }

        tenantContextAccessor.TenantId = tenant.Id;
        tenantContextAccessor.CurrentTenant = tenant;

        _logger.LogInformation("Request tenant resolved: {TenantId}", tenant.Id);
        await _next(context);
    }
}
