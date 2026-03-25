namespace DocApp.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendTemplateAsync(string to, string templateName, object model, string tenantId, CancellationToken cancellationToken = default);
}

public interface ISmsService
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}
