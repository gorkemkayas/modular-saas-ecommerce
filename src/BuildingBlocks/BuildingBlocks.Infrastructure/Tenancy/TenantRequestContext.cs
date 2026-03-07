namespace BuildingBlocks.Infrastructure.Tenancy
{
    public sealed class TenantRequestContext
    {
        public int? TenantId { get; set; }
        public string? TenantDomain { get; set; }
    }
}
