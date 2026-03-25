using DocApp.Application.Appointments.DTOs;
using DocApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DocApp.Application.Appointments.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeZoneService _tzService;

    public GetAppointmentsQueryHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        IDateTimeZoneService tzService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _tzService = tzService;
    }

    public async Task<List<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var timezone = request.UserTimezoneId ?? "UTC";

        var query = _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.TenantId == _tenantContext.TenantId)
            .AsQueryable();

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);

        if (request.PatientId.HasValue)
            query = query.Where(a => a.PatientId == request.PatientId.Value);

        if (request.From.HasValue)
            query = query.Where(a => a.StartTimeUtc >= Instant.FromDateTimeOffset(request.From.Value));

        if (request.To.HasValue)
            query = query.Where(a => a.StartTimeUtc <= Instant.FromDateTimeOffset(request.To.Value));

        var appointments = await query
            .OrderBy(a => a.StartTimeUtc)
            .ToListAsync(cancellationToken);

        return appointments.Select(a => new AppointmentDto(
            a.Id,
            a.TenantId,
            a.DoctorId,
            a.Doctor?.FullName ?? "Unknown",
            a.PatientId,
            a.Patient?.FullName ?? "Unknown",
            _tzService.ConvertFromUtc(a.StartTimeUtc, timezone),
            _tzService.ConvertFromUtc(a.EndTimeUtc, timezone),
            timezone,
            a.Status,
            a.Notes,
            a.CancellationReason
        )).ToList();
    }
}
