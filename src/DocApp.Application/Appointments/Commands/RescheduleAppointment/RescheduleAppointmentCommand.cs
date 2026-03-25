using MediatR;
using NodaTime;

namespace DocApp.Application.Appointments.Commands.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    Guid AppointmentId,
    LocalDateTime NewStartTimeLocal,
    string ClientTimezoneId
) : IRequest;
