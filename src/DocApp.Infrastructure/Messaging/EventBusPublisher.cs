using MassTransit;
using Microsoft.Extensions.Logging;

namespace DocApp.Infrastructure.Messaging;

/// <summary>Integration events published to the message bus (RabbitMQ/ASB).</summary>
public record AppointmentBookedIntegrationEvent(
    Guid AppointmentId,
    string TenantId,
    Guid DoctorId,
    Guid PatientId,
    string PatientEmail,
    string PatientName,
    string DoctorName,
    DateTimeOffset StartTime,
    string TimezoneId);

public record AppointmentCancelledIntegrationEvent(
    Guid AppointmentId,
    string TenantId,
    string PatientEmail,
    string PatientName,
    string Reason);

public record AppointmentRescheduledIntegrationEvent(
    Guid AppointmentId,
    string TenantId,
    string PatientEmail,
    DateTimeOffset OldStart,
    DateTimeOffset NewStart);

/// <summary>Thin wrapper over MassTransit IPublishEndpoint.</summary>
public sealed class EventBusPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EventBusPublisher> _logger;

    public EventBusPublisher(IPublishEndpoint publishEndpoint, ILogger<EventBusPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class
    {
        _logger.LogInformation("Publishing integration event {EventType}", typeof(T).Name);
        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
