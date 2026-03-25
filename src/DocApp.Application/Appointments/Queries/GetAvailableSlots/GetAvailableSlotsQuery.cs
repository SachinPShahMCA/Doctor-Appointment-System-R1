using DocApp.Application.Appointments.DTOs;
using DocApp.Application.Common.Interfaces;
using DocApp.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DocApp.Application.Appointments.Queries.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(
    Guid DoctorId,
    DateOnly Date,
    string UserTimezoneId
) : IRequest<List<AvailableSlotDto>>;

public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, List<AvailableSlotDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeZoneService _tzService;

    public GetAvailableSlotsQueryHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        IDateTimeZoneService tzService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _tzService = tzService;
    }

    public async Task<List<AvailableSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Availability)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.TenantId == _tenantContext.TenantId, cancellationToken);

        if (doctor is null) return [];

        var dayOfWeek = request.Date.DayOfWeek;
        var availability = doctor.Availability
            .FirstOrDefault(a => a.DayOfWeek == dayOfWeek && a.IsActive);

        if (availability is null) return [];

        // Get all booked slots for the day
        var tenantTz = _tenantContext.CurrentTenant?.Settings.DefaultTimezone ?? "UTC";
        var dayStartLocal = new LocalDateTime(request.Date.Year, request.Date.Month, request.Date.Day, 0, 0);
        var dayEndLocal = dayStartLocal.PlusHours(24);
        var dayStartUtc = _tzService.ConvertToUtc(dayStartLocal, tenantTz);
        var dayEndUtc = _tzService.ConvertToUtc(dayEndLocal, tenantTz);

        var bookedSlots = await _context.Appointments
            .Where(a =>
                a.DoctorId == request.DoctorId &&
                a.TenantId == _tenantContext.TenantId &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                a.StartTimeUtc >= dayStartUtc &&
                a.StartTimeUtc < dayEndUtc)
            .Select(a => new TimeSlot(a.StartTimeUtc, a.EndTimeUtc))
            .ToListAsync(cancellationToken);

        // Generate available time slots
        var slots = new List<AvailableSlotDto>();
        var current = new LocalDateTime(request.Date.Year, request.Date.Month, request.Date.Day,
            availability.StartTime.Hour, availability.StartTime.Minute);
        var end = new LocalDateTime(request.Date.Year, request.Date.Month, request.Date.Day,
            availability.EndTime.Hour, availability.EndTime.Minute);

        while (current.PlusMinutes(availability.SlotDurationMinutes) <= end)
        {
            var slotStart = _tzService.ConvertToUtc(current, tenantTz);
            var slotEnd = slotStart.Plus(Duration.FromMinutes(availability.SlotDurationMinutes));
            var slot = new TimeSlot(slotStart, slotEnd);

            if (!bookedSlots.Any(b => b.Overlaps(slot)))
            {
                slots.Add(new AvailableSlotDto(
                    _tzService.ConvertFromUtc(slotStart, request.UserTimezoneId),
                    _tzService.ConvertFromUtc(slotEnd, request.UserTimezoneId),
                    doctor.Id,
                    doctor.FullName));
            }

            current = current.PlusMinutes(availability.SlotDurationMinutes);
        }

        return slots;
    }
}
