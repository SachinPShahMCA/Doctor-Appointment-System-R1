using DocApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Appointment> Appointments { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<AppUser> AppUsers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
