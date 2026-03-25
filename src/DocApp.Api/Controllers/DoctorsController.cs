using DocApp.Application.Doctors.Commands.CreateDoctor;
using DocApp.Application.Doctors.DTOs;
using DocApp.Application.Doctors.Queries.GetDoctors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all doctors for the current tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DoctorDto>), 200)]
    public async Task<IActionResult> GetDoctors([FromQuery] string? specialty, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDoctorsQuery(specialty), ct);
        return Ok(result);
    }

    /// <summary>Create a new doctor (Admin only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequest request, CancellationToken ct)
    {
        var command = new CreateDoctorCommand(
            request.FullName,
            request.Email,
            request.Specialty,
            request.LicenseNumber);

        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetDoctors), new { id }, new { id });
    }
}

public sealed record CreateDoctorRequest(
    string FullName,
    string Email,
    string Specialty,
    string LicenseNumber
);
