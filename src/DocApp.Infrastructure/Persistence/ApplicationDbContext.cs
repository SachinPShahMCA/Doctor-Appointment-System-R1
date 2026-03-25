using DocApp.Application.Common.Interfaces;
using DocApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DocApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global tenant query filters (soft multi-tenancy via TenantId discriminator)
        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Doctor>()
            .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Patient>()
            .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<DoctorAvailability>()
            .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<AppUser>()
            .HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events BEFORE saving to DB
        var entitiesWithEvents = ChangeTracker
            .Entries<Domain.Entities.BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear events after save
        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        return result;
    }
}
