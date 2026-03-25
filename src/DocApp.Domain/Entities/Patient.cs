using NodaTime;

namespace DocApp.Domain.Entities;

public class Patient : AuditableEntity
{
    /// <summary>PII - should be encrypted at rest.</summary>
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public LocalDate? DateOfBirth { get; private set; }
    public string? PreferredTimezone { get; private set; }  // IANA timezone ID
    public string? PreferredLanguage { get; private set; }  // e.g. "en", "fr", "hi"
    public bool IsActive { get; private set; } = true;

    private Patient() { }

    public static Patient Create(
        string tenantId,
        string fullName,
        string email,
        string phone,
        LocalDate? dateOfBirth = null,
        string? preferredTimezone = null,
        string? preferredLanguage = null)
    {
        return new Patient
        {
            TenantId = tenantId,
            FullName = fullName,
            Email = email,
            PhoneNumber = phone,
            DateOfBirth = dateOfBirth,
            PreferredTimezone = preferredTimezone,
            PreferredLanguage = preferredLanguage ?? "en"
        };
    }

    public void Update(string fullName, string phone, string? tz, string? lang)
    {
        FullName = fullName;
        PhoneNumber = phone;
        PreferredTimezone = tz;
        PreferredLanguage = lang;
    }

    public void Deactivate() => IsActive = false;
}
