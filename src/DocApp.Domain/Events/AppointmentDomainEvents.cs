using NodaTime;

namespace DocApp.Domain.Events;

public record AppointmentBookedDomainEvent(
    Guid AppointmentId,
    string TenantId,
    Guid DoctorId,
    Guid PatientId,
    Instant StartTimeUtc);

public record AppointmentCancelledDomainEvent(
    Guid AppointmentId,
    string TenantId,
    string Reason);

public record AppointmentRescheduledDomainEvent(
    Guid AppointmentId,
    string TenantId,
    Instant OldStartTimeUtc,
    Instant NewStartTimeUtc);
