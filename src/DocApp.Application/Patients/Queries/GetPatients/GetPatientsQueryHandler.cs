using DocApp.Application.Common.Interfaces;
using DocApp.Application.Patients.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocApp.Application.Patients.Queries.GetPatients;

public sealed class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<PatientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PatientDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.FullName.ToLower().Contains(term) || 
                (p.Email != null && p.Email.ToLower().Contains(term)));
        }

        return await query
            .Select(p => new PatientDto(p.Id, p.FullName, p.Email, p.PhoneNumber, p.PreferredTimezone, p.PreferredLanguage))
            .ToListAsync(cancellationToken);
    }
}
