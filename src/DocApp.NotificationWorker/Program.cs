using DocApp.NotificationWorker.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console())
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // SendGrid
        services.AddSendGrid(o => o.ApiKey = cfg["SendGrid:ApiKey"]!);

        // MassTransit + RabbitMQ with retry + DLQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<EmailNotificationConsumer>();
            x.AddConsumer<AppointmentCancelledConsumer>();
            x.AddConsumer<SmsNotificationConsumer>();
            x.AddConsumer<SmsCancellationConsumer>();

            x.UsingRabbitMq((mctx, rcfg) =>
            {
                var rabbit = cfg.GetSection("RabbitMq");
                rcfg.Host(rabbit["Host"], rabbit["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbit["Username"] ?? "guest");
                    h.Password(rabbit["Password"] ?? "guest");
                });

                // Retry Policy: 5 retries with exponential backoff → Dead Letter Exchange
                rcfg.UseMessageRetry(r =>
                    r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

                rcfg.UseDelayedMessageScheduler();
                rcfg.ConfigureEndpoints(mctx);
            });
        });
    })
    .Build();

await host.RunAsync();
