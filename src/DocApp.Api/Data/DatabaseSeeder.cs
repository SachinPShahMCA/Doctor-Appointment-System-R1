using DocApp.Domain.Entities;
using DocApp.Domain.Enums;
using DocApp.Domain.ValueObjects;
using DocApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocApp.Api.Data;

/// <summary>
/// Seeds a demo tenant, admin user, sample doctor, and sample patient on first run.
/// Tech lead decision: Dev should work out-of-the-box without any manual DB setup.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Applying EF migrations...");
        await db.Database.MigrateAsync();

        // Bypass global query filter for seeding
        if (await db.Tenants.IgnoreQueryFilters().AnyAsync()) return;

        logger.LogInformation("Seeding demo data...");

        // ─── Tenant ──────────────────────────────────────────────────────────
        var tenant = Tenant.Create("demo", "Demo Clinic", new TenantSettings
        {
            PrimaryColor = "#4F46E5",
            DefaultTimezone = "Asia/Kolkata",
            DefaultLanguage = "en",
            EmailFromName = "Demo Clinic",
            EmailFromAddress = "noreply@demo-clinic.com"
        });
        db.Tenants.Add(tenant);

        // ─── Doctor ──────────────────────────────────────────────────────────
        var doctor = Doctor.Create(
            tenantId: "demo",
            fullName: "Dr. Sarah Johnson",
            specialty: "General Physician",
            email: "dr.sarah@demo-clinic.com",
            licenseNumber: "LIC-2024-001",
            phone: "+91-98765-43210");
        db.Doctors.Add(doctor);

        // Doctor availability: Mon–Fri, 9 AM – 5 PM, 30-min slots
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                                     DayOfWeek.Thursday, DayOfWeek.Friday })
        {
            db.DoctorAvailabilities.Add(DoctorAvailability.Create(
                tenantId: "demo",
                doctorId: doctor.Id,
                dayOfWeek: day,
                startTime: new TimeOnly(9, 0),
                endTime: new TimeOnly(17, 0),
                slotDurationMinutes: 30));
        }

        // ─── Patient ─────────────────────────────────────────────────────────
        var patient = Patient.Create(
            tenantId: "demo",
            fullName: "Rahul Mehta",
            email: "rahul@example.com",
            phone: "+91-91234-56789",
            preferredTimezone: "Asia/Kolkata",
            preferredLanguage: "en");
        db.Patients.Add(patient);

        // ─── Users (with hashed passwords) ───────────────────────────────────
        var hasher = new PasswordHasher<AppUser>();

        var adminUser = AppUser.Create("demo", "admin@demo-clinic.com", "Admin User", UserRole.Admin);
        var doctorUser = AppUser.Create("demo", "dr.sarah@demo-clinic.com", "Dr. Sarah Johnson", UserRole.Doctor, doctorId: doctor.Id);
        var patientUser = AppUser.Create("demo", "rahul@example.com", "Rahul Mehta", UserRole.Patient, patientId: patient.Id);

        db.AppUsers.Add(adminUser);
        db.AppUsers.Add(doctorUser);
        db.AppUsers.Add(patientUser);

        await db.SaveChangesAsync();

        // Store hashed passwords separately (in real app use ASP.NET Identity)
        // For demo: write to a simple lookup table via raw SQL
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "UserPasswords" (
                "UserId" uuid PRIMARY KEY,
                "PasswordHash" text NOT NULL
            );
            INSERT INTO "UserPasswords" ("UserId", "PasswordHash") VALUES
                ({0}, {1}),
                ({2}, {3}),
                ({4}, {5})
            ON CONFLICT DO NOTHING;
            """,
            adminUser.Id, hasher.HashPassword(adminUser, "Admin@123"),
            doctorUser.Id, hasher.HashPassword(doctorUser, "Doctor@123"),
            patientUser.Id, hasher.HashPassword(patientUser, "Patient@123"));

        logger.LogInformation("✅ Seed complete. Demo credentials:");
        logger.LogInformation("   Admin:   admin@demo-clinic.com  / Admin@123");
        logger.LogInformation("   Doctor:  dr.sarah@demo-clinic.com / Doctor@123");
        logger.LogInformation("   Patient: rahul@example.com / Patient@123");
        logger.LogInformation("   Tenant:  demo  (use X-Tenant-ID: demo header)");
    }
}
