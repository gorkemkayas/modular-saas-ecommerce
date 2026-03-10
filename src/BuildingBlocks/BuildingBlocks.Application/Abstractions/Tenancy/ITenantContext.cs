namespace BuildingBlocks.Application.Abstractions.Tenancy;

public interface ITenantContext
{
    int? TenantId { get; }
    Guid? TenantIdAsGuid { get; } // Yeni property
    bool HasTenant { get; }
}
