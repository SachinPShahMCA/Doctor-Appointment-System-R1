using NodaTime;

namespace DocApp.Domain.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Instant? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
