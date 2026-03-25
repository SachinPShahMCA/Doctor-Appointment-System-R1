using DocApp.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DocApp.NotificationWorker.Consumers;

public sealed class SmsNotificationConsumer : IConsumer<AppointmentBookedIntegrationEvent>
{
    private readonly ILogger<SmsNotificationConsumer> _logger;

    public SmsNotificationConsumer(ILogger<SmsNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentBookedIntegrationEvent> context)
    {
        var evt = context.Message;
        
        // In a real application, you would invoke a service like Twilio here to send SMS
        _logger.LogInformation("Sending SMS appointment confirmation to Patient: {PatientName}", evt.PatientName);

        // Simulated delay
        await Task.Delay(500);

        _logger.LogInformation("SMS confirmation successfully sent for Appointment {AppointmentId}", evt.AppointmentId);
    }
}

public sealed class SmsCancellationConsumer : IConsumer<AppointmentCancelledIntegrationEvent>
{
    private readonly ILogger<SmsCancellationConsumer> _logger;

    public SmsCancellationConsumer(ILogger<SmsCancellationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentCancelledIntegrationEvent> context)
    {
        var evt = context.Message;
        
        // In a real application, you would invoke a service like Twilio here to send SMS
        _logger.LogInformation("Sending SMS cancellation notice to Patient: {PatientName}", evt.PatientName);

        // Simulated delay
        await Task.Delay(500);

        _logger.LogInformation("SMS cancellation successfully sent for Appointment {AppointmentId}", evt.AppointmentId);
    }
}
