using DocApp.Application.Common.Interfaces;
using DocApp.Application.Doctors.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocApp.Application.Doctors.Queries.GetDoctors;

public sealed class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, List<DoctorDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Doctors.AsQueryable();

        // Note: Global query filter automatically applies TenantId
        if (!string.IsNullOrEmpty(request.Specialty))
        {
            query = query.Where(d => d.Specialty == request.Specialty);
        }

        return await query
            .Select(d => new DoctorDto(d.Id, d.FullName, d.Email, d.Specialty ?? string.Empty, d.LicenseNumber))
            .ToListAsync(cancellationToken);
    }
}
