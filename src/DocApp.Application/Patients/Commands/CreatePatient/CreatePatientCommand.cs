using MediatR;

namespace DocApp.Application.Patients.Commands.CreatePatient;

public sealed record CreatePatientCommand(
    string FullName,
    string? Email,
    string? PhoneNumber,
    string? PreferredTimezone,
    string? PreferredLanguage
) : IRequest<Guid>;
