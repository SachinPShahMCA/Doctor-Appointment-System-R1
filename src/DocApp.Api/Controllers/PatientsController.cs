using DocApp.Application.Patients.Commands.CreatePatient;
using DocApp.Application.Patients.DTOs;
using DocApp.Application.Patients.Queries.GetPatients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all patients for the current tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PatientDto>), 200)]
    public async Task<IActionResult> GetPatients([FromQuery] string? searchTerm, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientsQuery(searchTerm), ct);
        return Ok(result);
    }

    /// <summary>Register a new patient.</summary>
    [HttpPost]
    [AllowAnonymous] // Usually patients can self-register
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request, CancellationToken ct)
    {
        var command = new CreatePatientCommand(
            request.FullName,
            request.Email,
            request.PhoneNumber,
            request.PreferredTimezone,
            request.PreferredLanguage);

        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetPatients), new { id }, new { id });
    }
}

public sealed record CreatePatientRequest(
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? PreferredTimezone,
    string? PreferredLanguage
);
