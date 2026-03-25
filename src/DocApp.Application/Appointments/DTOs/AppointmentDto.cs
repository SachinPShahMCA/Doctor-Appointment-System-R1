using DocApp.Domain.Enums;

namespace DocApp.Application.Appointments.DTOs;

public sealed record AppointmentDto(
    Guid Id,
    string TenantId,
    Guid DoctorId,
    string DoctorName,
    Guid PatientId,
    string PatientName,
    DateTimeOffset StartTimeDisplay,   // Converted to user's timezone
    DateTimeOffset EndTimeDisplay,
    string TimezoneId,
    AppointmentStatus Status,
    string? Notes,
    string? CancellationReason
);

public sealed record AvailableSlotDto(
    DateTimeOffset Start,
    DateTimeOffset End,
    Guid DoctorId,
    string DoctorName
);
