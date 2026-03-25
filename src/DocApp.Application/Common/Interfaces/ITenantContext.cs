using DocApp.Domain.Entities;

namespace DocApp.Application.Common.Interfaces;

public interface ITenantContext
{
    string TenantId { get; }
    Tenant? CurrentTenant { get; }
}
