namespace ECommerce.API.Contracts.Store.ProvisionStoreForTenant
{
    public sealed record ProvisionStoreForTenantRequest(
        string Name,
        string? PlanCode = null);

    public sealed record ProvisionStoreForTenantResponse(
        string StoreId,
        string StoreSlug);
}
