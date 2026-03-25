using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Entities;
using DocApp.Domain.Exceptions;
using DocApp.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocApp.Application.Appointments.Commands.BookAppointment;

public sealed class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeZoneService _tzService;
    private readonly ICurrentUserService _currentUser;

    public BookAppointmentCommandHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        IDateTimeZoneService tzService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _tenantContext = tenantContext;
        _tzService = tzService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        // 1. DST-safe timezone conversion: LocalDateTime → UTC Instant
        //    ConvertToUtc throws InvalidTimezoneException, SkippedTimeException, AmbiguousTimeException
        var startUtc = _tzService.ConvertToUtc(request.StartTimeLocal, request.ClientTimezoneId);
        var endUtc = startUtc.Plus(NodaTime.Duration.FromMinutes(30));
        var requestedSlot = new TimeSlot(startUtc, endUtc);

        // 2. Verify doctor exists and is active
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.TenantId == _tenantContext.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), request.DoctorId);

        if (!doctor.IsActive)
            throw new DomainException($"Doctor '{doctor.FullName}' is not currently accepting appointments.");

        // 3. Check for overlapping appointments (conflict detection)
        var overlapping = await _context.Appointments
            .AnyAsync(a =>
                a.TenantId == _tenantContext.TenantId &&
                a.DoctorId == request.DoctorId &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                a.StartTimeUtc < endUtc &&
                a.EndTimeUtc > startUtc,
                cancellationToken);

        if (overlapping)
            throw new SlotUnavailableException(doctor.FullName, request.StartTimeLocal.ToString());

        // 4. Create aggregate root (raises AppointmentBookedDomainEvent internally)
        var appointment = Appointment.Create(
            _tenantContext.TenantId,
            request.DoctorId,
            request.PatientId,
            startUtc,
            endUtc,
            request.Notes);

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        return appointment.Id;
    }
}

// Custom exception needed since DomainException is abstract
file sealed class DomainException : global::DocApp.Domain.Exceptions.DomainException
{
    public DomainException(string message) : base(message) { }
}
