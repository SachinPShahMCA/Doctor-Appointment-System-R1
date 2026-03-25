using MediatR;

namespace DocApp.Application.Doctors.Commands.CreateDoctor;

public sealed record CreateDoctorCommand(
    string FullName,
    string Email,
    string Specialty,
    string LicenseNumber
) : IRequest<Guid>;
