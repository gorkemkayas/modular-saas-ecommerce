using Catalog.Application.Products.Queries.GetProductById;
using Catalog.Domain.Enums;
using MediatR;
using Pricing.Application.Integrations;

namespace ECommerce.API.Integrations.Pricing;

public sealed class CatalogSellableItemValidator : ICatalogSellableItemValidator
{
    private readonly ISender _sender;

    public CatalogSellableItemValidator(ISender sender)
    {
        _sender = sender;
    }

    public async Task<SellableItemValidationResult> ValidateAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default)
    {
        var product = await _sender.Send(new GetProductByIdQuery(storeId, productId), cancellationToken);

        if (product is null)
        {
            return new SellableItemValidationResult(
                ProductExists: false,
                VariantExists: false,
                VariantBelongsToProduct: false,
                VariantIsActive: false,
                ProductType: CatalogSellableItemType.Unknown);
        }

        var resolvedProductType = product.ProductType == ProductType.Simple
            ? CatalogSellableItemType.Simple
            : CatalogSellableItemType.Variant;

        if (!productVariantId.HasValue)
        {
            return new SellableItemValidationResult(
                ProductExists: true,
                VariantExists: false,
                VariantBelongsToProduct: false,
                VariantIsActive: false,
                ProductType: resolvedProductType);
        }

        var variant = product.Variants.FirstOrDefault(x => x.Id == productVariantId.Value);

        return new SellableItemValidationResult(
            ProductExists: true,
            VariantExists: variant is not null,
            VariantBelongsToProduct: variant is not null && variant.ProductId == productId,
            VariantIsActive: variant?.IsActive ?? false,
            ProductType: resolvedProductType);
    }
}
