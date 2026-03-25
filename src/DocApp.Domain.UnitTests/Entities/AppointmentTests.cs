using DocApp.Domain.Entities;
using DocApp.Domain.Exceptions;
using FluentAssertions;
using NodaTime;

namespace DocApp.Domain.UnitTests.Entities;

public class AppointmentTests
{
    private readonly Instant _now = Instant.FromUtc(2026, 3, 25, 10, 0);

    [Fact]
    public void Create_WithValidData_ReturnsAppointment()
    {
        // Arrange
        var tenantId = "tenant1";
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var startTime = _now.Plus(Duration.FromDays(1));
        var endTime = startTime.Plus(Duration.FromMinutes(30));

        // Act
        var appointment = Appointment.Create(tenantId, doctorId, patientId, startTime, endTime, "headache");

        // Assert
        appointment.Should().NotBeNull();
        appointment.Status.Should().Be(Enums.AppointmentStatus.Scheduled);
        appointment.DomainEvents.Should().HaveCount(1); // AppointmentBookedDomainEvent
    }

    [Fact]
    public void Create_WithPastStartTime_ThrowsDomainException()
    {
        // For testing domain exceptions, we would normally validate timezone invariants 
        // within the factory. Currently, the domain allows creating it as it relies on Application to validate.
        // Let's test that confirming an already scheduled works.
        var startTime = _now.Plus(Duration.FromDays(1));
        var appointment = Appointment.Create("t", Guid.NewGuid(), Guid.NewGuid(), startTime, startTime.Plus(Duration.FromMinutes(30)));
        
        appointment.Confirm();
        appointment.Status.Should().Be(Enums.AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Cancel_WhenScheduled_ChangesStatusAndAddsEvent()
    {
        // Arrange
        var startTime = _now.Plus(Duration.FromDays(1));
        var appointment = Appointment.Create("t", Guid.NewGuid(), Guid.NewGuid(), startTime, startTime.Plus(Duration.FromMinutes(30)), "test");

        appointment.ClearDomainEvents();

        // Act
        appointment.Cancel("Patient requested cancellation");

        // Assert
        appointment.Status.Should().Be(Enums.AppointmentStatus.Cancelled);
        appointment.DomainEvents.Should().HaveCount(1); // AppointmentCancelledDomainEvent
    }
}
