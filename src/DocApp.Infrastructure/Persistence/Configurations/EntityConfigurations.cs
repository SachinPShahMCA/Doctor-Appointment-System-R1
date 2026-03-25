using DocApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocApp.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);

        // Composite index for conflict detection queries
        builder.HasIndex(a => new { a.TenantId, a.DoctorId, a.StartTimeUtc })
            .HasDatabaseName("IX_Appointments_TenantId_DoctorId_Start");

        builder.HasIndex(a => a.StartTimeUtc)
            .HasDatabaseName("IX_Appointments_StartTimeUtc");

        builder.HasOne<Doctor>().WithMany().HasForeignKey(a => a.DoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>().WithMany().HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.TenantId).HasMaxLength(100).IsRequired();
        builder.Property(d => d.FullName).HasMaxLength(255).IsRequired();
        builder.Property(d => d.Email).HasMaxLength(255).IsRequired();
        builder.Property(d => d.LicenseNumber).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Specialty).HasMaxLength(100);

        builder.HasIndex(d => new { d.TenantId, d.Email }).IsUnique()
            .HasDatabaseName("IX_Doctors_TenantId_Email");

        builder.HasMany(d => d.Availability)
            .WithOne()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TenantId).HasMaxLength(100).IsRequired();
        builder.Property(p => p.FullName).HasMaxLength(255).IsRequired();

        // PII — mark for column-level encryption in production via Always Encrypted
        builder.Property(p => p.Email).HasMaxLength(255);
        builder.Property(p => p.PhoneNumber).HasMaxLength(30);
        builder.Property(p => p.PreferredTimezone).HasMaxLength(100);
        builder.Property(p => p.PreferredLanguage).HasMaxLength(10);

        builder.HasIndex(p => new { p.TenantId, p.Email })
            .HasDatabaseName("IX_Patients_TenantId_Email");
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(100);
        builder.Property(t => t.Name).HasMaxLength(255).IsRequired();

        // TenantSettings as a JSON-owned column (PostgreSQL JSONB)
        builder.OwnsOne(t => t.Settings, s =>
        {
            s.ToJson();
        });
    }
}
