using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocApp.Application.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CancelAppointmentCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.TenantId == _tenantContext.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Appointment), request.AppointmentId);

        appointment.Cancel(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
