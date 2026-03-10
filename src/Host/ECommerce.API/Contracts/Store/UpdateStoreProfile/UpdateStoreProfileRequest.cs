namespace ECommerce.API.Contracts.Store.UpdateStoreProfile
{
    public sealed record UpdateStoreProfileRequest(string Name, string? Description, string? LogoUrl);
}
