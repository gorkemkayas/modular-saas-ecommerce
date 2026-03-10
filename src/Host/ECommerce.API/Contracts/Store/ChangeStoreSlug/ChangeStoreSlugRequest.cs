namespace ECommerce.API.Contracts.Store.ChangeStoreSlug
{
    public sealed record ChangeStoreSlugRequest(Guid TenantId, string NewSlug);
}
