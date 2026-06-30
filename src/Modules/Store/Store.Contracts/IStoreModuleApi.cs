namespace Store.Contracts;

public interface IStoreModuleApi
{
    Task<StoreBranding?> GetBrandingAsync(
        Guid storeId,
        CancellationToken cancellationToken = default);
}

public sealed record StoreBranding(
    Guid StoreId,
    string Name,
    string? LogoUrl);
