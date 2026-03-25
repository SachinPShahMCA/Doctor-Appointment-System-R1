namespace DocApp.Domain.Entities;

public class DoctorAvailability : BaseEntity
{
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>Local time of day. Interpreted in tenant's default timezone.</summary>
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; } = 30;
    public bool IsActive { get; private set; } = true;

    private DoctorAvailability() { }

    public static DoctorAvailability Create(
        string tenantId,
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes = 30)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.");

        return new DoctorAvailability
        {
            TenantId = tenantId,
            DoctorId = doctorId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            SlotDurationMinutes = slotDurationMinutes
        };
    }
}
