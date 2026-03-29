using Catalog.Domain.Enums;

namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductSummaryDto(
        Guid Id,
        string Name,
        string Slug,
        string? ShortDescription,
        Guid? BrandId,
        string? BrandName,
        ProductType ProductType,
        DateTime? PublishedAtUtc,
        string? MainImageUrl);
}
