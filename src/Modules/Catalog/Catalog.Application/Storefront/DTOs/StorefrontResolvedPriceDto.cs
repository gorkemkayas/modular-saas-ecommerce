namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontResolvedPriceDto(
        Guid ProductId,
        Guid? ProductVariantId,
        decimal Amount,
        string CurrencyCode,
        decimal? CompareAtAmount,
        bool IsOnSale);
}
