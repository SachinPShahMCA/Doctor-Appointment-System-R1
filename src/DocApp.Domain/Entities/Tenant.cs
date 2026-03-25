using DocApp.Domain.ValueObjects;

namespace DocApp.Domain.Entities;

public class Tenant
{
    public string Id { get; private set; } = string.Empty;       // Slug: "clinic-alpha"
    public string Name { get; private set; } = string.Empty;
    public string? ConnectionString { get; private set; }         // Optional per-tenant DB override
    public TenantSettings Settings { get; private set; } = new();
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public static Tenant Create(string id, string name, TenantSettings? settings = null)
    {
        return new Tenant
        {
            Id = id.ToLowerInvariant().Trim(),
            Name = name,
            Settings = settings ?? new TenantSettings()
        };
    }

    public void UpdateSettings(TenantSettings settings) => Settings = settings;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
