using Catalog.Domain.Enums;

namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductMediaDto(
        Guid Id,
        Guid? ProductVariantId,
        MediaType MediaType,
        string Url,
        string? AltText,
        bool IsMain,
        int SortOrder);
}
