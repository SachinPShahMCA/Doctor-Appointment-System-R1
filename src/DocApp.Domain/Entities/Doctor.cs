namespace DocApp.Domain.Entities;

public class Doctor : AuditableEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public ICollection<DoctorAvailability> Availability { get; private set; } = [];

    private Doctor() { }

    public static Doctor Create(string tenantId, string fullName, string specialty, string email, string licenseNumber, string phone)
    {
        return new Doctor
        {
            TenantId = tenantId,
            FullName = fullName,
            Specialty = specialty,
            Email = email,
            LicenseNumber = licenseNumber,
            PhoneNumber = phone
        };
    }

    public void Update(string fullName, string specialty, string phone)
    {
        FullName = fullName;
        Specialty = specialty;
        PhoneNumber = phone;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
