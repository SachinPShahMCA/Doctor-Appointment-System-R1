namespace DocApp.Domain.ValueObjects;

/// <summary>
/// Tenant white-label and configuration settings. Stored as an owned JSON column.
/// </summary>
public class TenantSettings
{
    public string PrimaryColor { get; set; } = "#4F46E5";
    public string SecondaryColor { get; set; } = "#7C3AED";
    public string LogoUrl { get; set; } = string.Empty;
    public string FaviconUrl { get; set; } = string.Empty;
    public string DefaultTimezone { get; set; } = "UTC";         // IANA timezone ID
    public string DefaultLanguage { get; set; } = "en";
    public string[] SupportedLanguages { get; set; } = ["en"];
    public string CustomDomain { get; set; } = string.Empty;      // e.g. clinic.example.com
    public string EmailFromName { get; set; } = "DocApp";
    public string EmailFromAddress { get; set; } = "noreply@docapp.io";
    public bool SmsNotificationsEnabled { get; set; } = true;
    public bool WhatsAppNotificationsEnabled { get; set; } = false;
    public int DefaultSlotDurationMinutes { get; set; } = 30;
    public int MaxBookingDaysAhead { get; set; } = 60;
}
