using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Products.DTOs;
using Catalog.Contracts;
using Catalog.Domain.Enums;
namespace Catalog.Application.Contracts;

public sealed class CatalogModuleApi : ICatalogModuleApi
{
    private readonly IProductReadService _productReadService;

    public CatalogModuleApi(IProductReadService productReadService)
    {
        _productReadService = productReadService;
    }

    public async Task<CatalogSellableItemResult?> GetSellableItemAsync(
        GetCatalogSellableItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productReadService.GetByIdAsync(
            request.StoreId,
            request.ProductId,
            cancellationToken);

        if (product is null)
            return null;

        if (product.ProductType == ProductType.Variant)
        {
            if (!request.ProductVariantId.HasValue)
                return null;

            var variant = product.Variants.FirstOrDefault(x => x.Id == request.ProductVariantId.Value && x.IsActive);
            if (variant is null)
                return null;

            return new CatalogSellableItemResult(
                product.Id,
                variant.Id,
                product.Name,
                variant.Name,
                variant.Sku,
                ResolvePrimaryImageUrl(product, variant.Id));
        }

        if (request.ProductVariantId.HasValue)
            return null;

        return new CatalogSellableItemResult(
            product.Id,
            null,
            product.Name,
            null,
            product.Sku ?? string.Empty,
            ResolvePrimaryImageUrl(product, null));
    }

    private static string? ResolvePrimaryImageUrl(ProductDto product, Guid? variantId)
    {
        var images = product.MediaItems
            .Where(x => x.MediaType == MediaType.Image)
            .ToArray();

        if (images.Length == 0)
            return null;

        if (variantId.HasValue)
        {
            var variantImage = images
                .Where(x => x.ProductVariantId == variantId)
                .OrderByDescending(x => x.IsMain)
                .ThenBy(x => x.SortOrder)
                .FirstOrDefault();

            if (variantImage is not null)
                return variantImage.Url;
        }

        var productImage = images
            .Where(x => x.ProductVariantId is null)
            .OrderByDescending(x => x.IsMain)
            .ThenBy(x => x.SortOrder)
            .FirstOrDefault();

        return (productImage ?? images
                .OrderByDescending(x => x.IsMain)
                .ThenBy(x => x.SortOrder)
                .First())
            .Url;
    }

    public async Task<CatalogSellableItemValidationResult> ValidateSellableItemAsync(
        ValidateCatalogSellableItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productReadService.GetByIdAsync(
            request.StoreId,
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            return new CatalogSellableItemValidationResult(
                ProductExists: false,
                VariantExists: false,
                VariantBelongsToProduct: false,
                VariantIsActive: false,
                ProductType: CatalogSellableItemType.Unknown);
        }

        var resolvedProductType = product.ProductType == ProductType.Simple
            ? CatalogSellableItemType.Simple
            : CatalogSellableItemType.Variant;

        if (!request.ProductVariantId.HasValue)
        {
            return new CatalogSellableItemValidationResult(
                ProductExists: true,
                VariantExists: false,
                VariantBelongsToProduct: false,
                VariantIsActive: false,
                ProductType: resolvedProductType);
        }

        var variant = product.Variants.FirstOrDefault(x => x.Id == request.ProductVariantId.Value);

        return new CatalogSellableItemValidationResult(
            ProductExists: true,
            VariantExists: variant is not null,
            VariantBelongsToProduct: variant is not null && variant.ProductId == product.Id,
            VariantIsActive: variant?.IsActive ?? false,
            ProductType: resolvedProductType);
    }
}
