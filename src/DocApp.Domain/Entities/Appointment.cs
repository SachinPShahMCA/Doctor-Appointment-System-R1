using DocApp.Domain.Enums;
using DocApp.Domain.Events;
using NodaTime;

namespace DocApp.Domain.Entities;

public class Appointment : AuditableEntity
{
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }

    /// <summary>Always stored in UTC using NodaTime.Instant.</summary>
    public Instant StartTimeUtc { get; private set; }
    public Instant EndTimeUtc { get; private set; }

    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }

    // Navigation properties (EF Core eager loading)
    public Doctor? Doctor { get; private set; }
    public Patient? Patient { get; private set; }

    // EF Core parameterless ctor
    private Appointment() { }

    public static Appointment Create(
        string tenantId,
        Guid doctorId,
        Guid patientId,
        Instant startUtc,
        Instant endUtc,
        string? notes = null)
    {
        var appointment = new Appointment
        {
            TenantId = tenantId,
            DoctorId = doctorId,
            PatientId = patientId,
            StartTimeUtc = startUtc,
            EndTimeUtc = endUtc,
            Status = AppointmentStatus.Scheduled,
            Notes = notes,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        appointment.AddDomainEvent(new AppointmentBookedDomainEvent(appointment.Id, tenantId, doctorId, patientId, startUtc));
        return appointment;
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled appointments can be confirmed.");
        Status = AppointmentStatus.Confirmed;
    }

    public void Cancel(string reason)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled appointment.");
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        AddDomainEvent(new AppointmentCancelledDomainEvent(Id, TenantId, reason));
    }

    public void Reschedule(Instant newStartUtc, Instant newEndUtc)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule a completed or cancelled appointment.");
        var oldStart = StartTimeUtc;
        StartTimeUtc = newStartUtc;
        EndTimeUtc = newEndUtc;
        Status = AppointmentStatus.Rescheduled;
        AddDomainEvent(new AppointmentRescheduledDomainEvent(Id, TenantId, oldStart, newStartUtc));
    }

    public void Complete() => Status = AppointmentStatus.Completed;
    public void MarkNoShow() => Status = AppointmentStatus.NoShow;
}
