using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Entities;
using MediatR;

namespace DocApp.Application.Patients.Commands.CreatePatient;

public sealed class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreatePatientCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = Domain.Entities.Patient.Create(
            tenantId: _tenantContext.TenantId,
            fullName: request.FullName,
            email: request.Email ?? string.Empty,
            phone: request.PhoneNumber ?? string.Empty,
            dateOfBirth: null,
            preferredTimezone: request.PreferredTimezone ?? "UTC",
            preferredLanguage: request.PreferredLanguage ?? "en");

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}
