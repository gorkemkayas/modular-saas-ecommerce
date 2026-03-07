namespace BuildingBlocks.Application.Abstractions.Tenancy
{
    public interface ITenantContext
    {
        int? TenantId { get; }
        string? TenantDomain { get; }
        bool HasTenant { get; }
    }
}
