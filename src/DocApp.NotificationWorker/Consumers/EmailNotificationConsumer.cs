using DocApp.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Scriban;

namespace DocApp.NotificationWorker.Consumers;

public sealed class EmailNotificationConsumer : IConsumer<AppointmentBookedIntegrationEvent>
{
    private readonly ISendGridClient _sendGrid;
    private readonly ILogger<EmailNotificationConsumer> _logger;

    public EmailNotificationConsumer(ISendGridClient sendGrid, ILogger<EmailNotificationConsumer> logger)
    {
        _sendGrid = sendGrid;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentBookedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Sending appointment confirmation email to {Email}", evt.PatientEmail);

        var templateSource = await LoadTemplateAsync("appointment_confirmation");
        var template = Template.Parse(templateSource);
        var htmlBody = await template.RenderAsync(new
        {
            patient_name = evt.PatientName,
            doctor_name = evt.DoctorName,
            start_time = evt.StartTime.ToString("f"),
            timezone = evt.TimezoneId,
            tenant_id = evt.TenantId
        });

        var msg = MailHelper.CreateSingleEmail(
            from: new EmailAddress("noreply@docapp.io", "DocApp"),
            to: new EmailAddress(evt.PatientEmail, evt.PatientName),
            subject: "Your Appointment is Confirmed ✅",
            plainTextContent: $"Hi {evt.PatientName}, your appointment with Dr. {evt.DoctorName} is confirmed on {evt.StartTime:f}.",
            htmlContent: htmlBody
        );

        var response = await _sendGrid.SendEmailAsync(msg, context.CancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("SendGrid failed with status {Status}", response.StatusCode);
            throw new Exception($"Email delivery failed: {response.StatusCode}");
            // MassTransit will retry and eventually dead-letter after retries exhausted
        }

        _logger.LogInformation("Appointment confirmation email sent to {Email}", evt.PatientEmail);
    }

    private static async Task<string> LoadTemplateAsync(string templateName)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{templateName}.html");
        return await File.ReadAllTextAsync(path);
    }
}

public sealed class AppointmentCancelledConsumer : IConsumer<AppointmentCancelledIntegrationEvent>
{
    private readonly ISendGridClient _sendGrid;
    private readonly ILogger<AppointmentCancelledConsumer> _logger;

    public AppointmentCancelledConsumer(ISendGridClient sendGrid, ILogger<AppointmentCancelledConsumer> logger)
    {
        _sendGrid = sendGrid;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentCancelledIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Sending cancellation email to {Email}", evt.PatientEmail);

        var msg = MailHelper.CreateSingleEmail(
            from: new EmailAddress("noreply@docapp.io", "DocApp"),
            to: new EmailAddress(evt.PatientEmail, evt.PatientName),
            subject: "Appointment Cancelled",
            plainTextContent: $"Hi {evt.PatientName}, your appointment has been cancelled. Reason: {evt.Reason}",
            htmlContent: $"<p>Hi <b>{evt.PatientName}</b>, your appointment has been <b>cancelled</b>.<br/>Reason: {evt.Reason}</p>"
        );

        await _sendGrid.SendEmailAsync(msg, context.CancellationToken);
    }
}
