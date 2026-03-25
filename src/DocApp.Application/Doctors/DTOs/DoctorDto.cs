namespace DocApp.Application.Doctors.DTOs;

public sealed record DoctorDto(
    Guid Id,
    string FullName,
    string Email,
    string Specialty,
    string LicenseNumber
);
