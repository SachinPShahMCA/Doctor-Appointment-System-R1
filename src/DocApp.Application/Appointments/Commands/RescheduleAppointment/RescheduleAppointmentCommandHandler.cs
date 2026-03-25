using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Exceptions;
using DocApp.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DocApp.Application.Appointments.Commands.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeZoneService _tzService;

    public RescheduleAppointmentCommandHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        IDateTimeZoneService tzService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _tzService = tzService;
    }

    public async Task Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.TenantId == _tenantContext.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Appointment), request.AppointmentId);

        var newStartUtc = _tzService.ConvertToUtc(request.NewStartTimeLocal, request.ClientTimezoneId);
        var newEndUtc = newStartUtc.Plus(Duration.FromMinutes(30));

        // Conflict check (exclude current appointment)
        var conflicting = await _context.Appointments
            .AnyAsync(a =>
                a.Id != appointment.Id &&
                a.TenantId == _tenantContext.TenantId &&
                a.DoctorId == appointment.DoctorId &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                a.StartTimeUtc < newEndUtc &&
                a.EndTimeUtc > newStartUtc,
                cancellationToken);

        if (conflicting)
            throw new SlotUnavailableException("Doctor", request.NewStartTimeLocal.ToString());

        appointment.Reschedule(newStartUtc, newEndUtc);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
