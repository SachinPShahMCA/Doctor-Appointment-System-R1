using MediatR;

namespace DocApp.Application.Appointments.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(Guid AppointmentId, string Reason) : IRequest;
