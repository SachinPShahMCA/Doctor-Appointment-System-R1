namespace DocApp.Application.Patients.DTOs;

public sealed record PatientDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? PreferredTimezone,
    string? PreferredLanguage
);
