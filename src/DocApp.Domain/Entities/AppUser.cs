using DocApp.Domain.Enums;

namespace DocApp.Domain.Entities;

public class AppUser : AuditableEntity
{
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public Guid? DoctorId { get; private set; }
    public Guid? PatientId { get; private set; }
    public string? PreferredLanguage { get; private set; } = "en";
    public bool IsActive { get; private set; } = true;

    private AppUser() { }

    public static AppUser Create(string tenantId, string email, string fullName, UserRole role,
        Guid? doctorId = null, Guid? patientId = null)
    {
        return new AppUser
        {
            TenantId = tenantId,
            Email = email.ToLowerInvariant().Trim(),
            FullName = fullName,
            Role = role,
            DoctorId = doctorId,
            PatientId = patientId
        };
    }

    public void Deactivate() => IsActive = false;
    public void SetLanguage(string lang) => PreferredLanguage = lang;
}
