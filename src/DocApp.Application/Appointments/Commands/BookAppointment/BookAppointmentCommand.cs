using MediatR;
using NodaTime;

namespace DocApp.Application.Appointments.Commands.BookAppointment;

public sealed record BookAppointmentCommand(
    Guid DoctorId,
    Guid PatientId,
    LocalDateTime StartTimeLocal,     // Client's local time (no tz info)
    string ClientTimezoneId,          // IANA timezone e.g. "America/New_York"
    string? Notes = null
) : IRequest<Guid>;
