using DocApp.Application.Common.Interfaces;
using DocApp.Infrastructure.Messaging;
using DocApp.Infrastructure.Persistence;
using DocApp.Infrastructure.Services;
using DocApp.Infrastructure.Time;
using Microsoft.Extensions.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL with NodaTime
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.UseNodaTime()));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Tenant
        services.AddScoped<TenantContextAccessor>();
        services.AddScoped<ITenantContext>(provider =>
            provider.GetRequiredService<TenantContextAccessor>());
        services.AddScoped<TenantResolutionService>();

        // User
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Timezone
        services.AddSingleton<IDateTimeZoneService, DateTimeZoneService>();

        // Redis — optional in dev (gracefully skipped if not configured)
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn))
        {
            services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConn);
        }
        else
        {
            // Fall back to in-memory cache so tenant resolution still works
            services.AddDistributedMemoryCache();
        }

        // MassTransit + RabbitMQ — optional in dev
        var rabbitHost = configuration["RabbitMq:Host"];
        if (!string.IsNullOrEmpty(rabbitHost))
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var rabbit = configuration.GetSection("RabbitMq");
                    cfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                    {
                        h.Username(rabbit["Username"] ?? "guest");
                        h.Password(rabbit["Password"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
            });
        }
        else
        {
            // No-op bus for dev — commands still work, events just not published
            services.AddMassTransit(x => x.UsingInMemory());
        }

        services.AddScoped<EventBusPublisher>();

        return services;
    }
}
