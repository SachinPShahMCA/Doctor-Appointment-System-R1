using DocApp.Application.Appointments.DTOs;
using MediatR;

namespace DocApp.Application.Appointments.Queries.GetAppointments;

public sealed record GetAppointmentsQuery(
    string? UserTimezoneId = "UTC",
    Guid? DoctorId = null,
    Guid? PatientId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null
) : IRequest<List<AppointmentDto>>;
