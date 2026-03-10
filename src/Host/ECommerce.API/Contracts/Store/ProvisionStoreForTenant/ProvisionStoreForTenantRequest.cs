namespace ECommerce.API.Contracts.Store.ProvisionStoreForTenant
{
    public sealed record ProvisionStoreForTenantRequest(
    Guid TenantId,
    string Name,
    string Slug);
}
