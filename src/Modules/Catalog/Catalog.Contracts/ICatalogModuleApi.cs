namespace Catalog.Contracts;

public interface ICatalogModuleApi
{
    Task<CatalogSellableItemResult?> GetSellableItemAsync(
        GetCatalogSellableItemRequest request,
        CancellationToken cancellationToken = default);

    Task<CatalogSellableItemValidationResult> ValidateSellableItemAsync(
        ValidateCatalogSellableItemRequest request,
        CancellationToken cancellationToken = default);
}
