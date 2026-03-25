using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DocApp.Infrastructure.Services;

/// <summary>
/// Scoped tenant context. Set by TenantMiddleware at request start.
/// </summary>
public sealed class TenantContextAccessor : ITenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public Tenant? CurrentTenant { get; set; }
}

/// <summary>
/// Resolves the tenant from request HTTP context (header or subdomain).
/// Caches tenant data in Redis.
/// </summary>
public sealed class TenantResolutionService
{
    private readonly IApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TenantResolutionService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public TenantResolutionService(
        IApplicationDbContext db,
        IDistributedCache cache,
        ILogger<TenantResolutionService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Tenant?> ResolveAsync(HttpContext context)
    {
        // Priority 1: X-Tenant-ID header
        var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();

        // Priority 2: Subdomain extraction (e.g., clinic-alpha.docapp.io → "clinic-alpha")
        if (string.IsNullOrEmpty(tenantId))
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length >= 3)
                tenantId = parts[0]; // e.g. "clinic-alpha"
        }

        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID found in request.");
            return null;
        }

        // Check Redis cache first
        var cacheKey = $"tenant:{tenantId}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<Tenant>(cached);
        }

        // Fallback to DB
        var tenant = _db.Tenants.FirstOrDefault(t => t.Id == tenantId && t.IsActive);
        if (tenant is null)
        {
            _logger.LogWarning("Tenant '{TenantId}' not found or inactive.", tenantId);
            return null;
        }

        // Cache the result
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tenant),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });

        return tenant;
    }
}
