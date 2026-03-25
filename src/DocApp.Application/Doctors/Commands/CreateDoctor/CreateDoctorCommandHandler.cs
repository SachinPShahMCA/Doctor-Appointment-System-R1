using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Entities;
using MediatR;

namespace DocApp.Application.Doctors.Commands.CreateDoctor;

public sealed class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateDoctorCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = Domain.Entities.Doctor.Create(
            _tenantContext.TenantId,
            request.FullName,
            request.Specialty,
            request.Email,
            request.LicenseNumber,
            null); // Phone is optional

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        return doctor.Id;
    }
}
