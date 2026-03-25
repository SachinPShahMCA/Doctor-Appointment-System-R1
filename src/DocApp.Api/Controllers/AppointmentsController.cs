using DocApp.Application.Appointments.Commands.BookAppointment;
using DocApp.Application.Appointments.Commands.CancelAppointment;
using DocApp.Application.Appointments.Commands.RescheduleAppointment;
using DocApp.Application.Appointments.Queries.GetAppointments;
using DocApp.Application.Appointments.Queries.GetAvailableSlots;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace DocApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Book a new appointment.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request, CancellationToken ct)
    {
        var command = new BookAppointmentCommand(
            request.DoctorId,
            request.PatientId,
            LocalDateTime.FromDateTime(request.StartTimeLocal.DateTime),
            request.ClientTimezoneId,
            request.Notes);

        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAppointments), new { id }, new { id });
    }

    /// <summary>Get all appointments for the current tenant (filtered).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string timezone = "UTC",
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAppointmentsQuery(timezone, doctorId, patientId, from, to), ct);
        return Ok(result);
    }

    /// <summary>Get available slots for a doctor on a given date.</summary>
    [HttpGet("slots")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetSlots(
        [FromQuery] Guid doctorId,
        [FromQuery] DateOnly date,
        [FromQuery] string timezone = "UTC",
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAvailableSlotsQuery(doctorId, date, timezone), ct);
        return Ok(result);
    }

    /// <summary>Cancel an appointment.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRequest request, CancellationToken ct)
    {
        await _mediator.Send(new CancelAppointmentCommand(id, request.Reason), ct);
        return NoContent();
    }

    /// <summary>Reschedule an appointment.</summary>
    [HttpPut("{id:guid}/reschedule")]
    [ProducesResponseType(204)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleRequest request, CancellationToken ct)
    {
        await _mediator.Send(new RescheduleAppointmentCommand(
            id,
            LocalDateTime.FromDateTime(request.NewStartTimeLocal.DateTime),
            request.ClientTimezoneId), ct);
        return NoContent();
    }
}

// ─── Request models ──────────────────────────────────────────────────────────

public sealed record BookAppointmentRequest(
    Guid DoctorId,
    Guid PatientId,
    DateTimeOffset StartTimeLocal,
    string ClientTimezoneId,
    string? Notes = null);

public sealed record CancelRequest(string Reason);

public sealed record RescheduleRequest(
    DateTimeOffset NewStartTimeLocal,
    string ClientTimezoneId);
