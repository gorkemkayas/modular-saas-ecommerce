using Catalog.Contracts;
using CatalogContractSellableItemType = Catalog.Contracts.CatalogSellableItemType;
using PricingCatalogSellableItemType = Pricing.Application.Integrations.CatalogSellableItemType;
using Pricing.Application.Integrations;

namespace Pricing.Infrastructure.Integrations.Catalog;

public sealed class CatalogSellableItemValidator : ICatalogSellableItemValidator
{
    private readonly ICatalogModuleApi _catalogModuleApi;

    public CatalogSellableItemValidator(ICatalogModuleApi catalogModuleApi)
    {
        _catalogModuleApi = catalogModuleApi;
    }

    public async Task<SellableItemValidationResult> ValidateAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalogModuleApi.ValidateSellableItemAsync(
            new ValidateCatalogSellableItemRequest(storeId, productId, productVariantId),
            cancellationToken);

        return new SellableItemValidationResult(
            result.ProductExists,
            result.VariantExists,
            result.VariantBelongsToProduct,
            result.VariantIsActive,
            MapProductType(result.ProductType));
    }

    private static PricingCatalogSellableItemType MapProductType(CatalogContractSellableItemType productType)
    {
        return productType switch
        {
            CatalogContractSellableItemType.Simple => PricingCatalogSellableItemType.Simple,
            CatalogContractSellableItemType.Variant => PricingCatalogSellableItemType.Variant,
            _ => PricingCatalogSellableItemType.Unknown
        };
    }
}
