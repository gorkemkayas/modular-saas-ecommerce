namespace BuildingBlocks.Application.Abstractions.Tenancy
{
    public interface ITenantContext
    {
        int? TenantId { get; }
        bool HasTenant { get; }
    }
}
