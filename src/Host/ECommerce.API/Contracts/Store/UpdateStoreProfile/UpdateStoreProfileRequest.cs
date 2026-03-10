namespace ECommerce.API.Contracts.Store.UpdateStoreProfile
{
    public sealed record UpdateStoreProfileRequest(Guid TenantId, string Name, string? Description, string? LogoUrl);
}
