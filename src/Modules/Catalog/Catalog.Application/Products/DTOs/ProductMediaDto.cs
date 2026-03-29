using Catalog.Domain.Enums;

namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductMediaDto(
        Guid Id,
        Guid ProductId,
        Guid? ProductVariantId,
        MediaType MediaType,
        string Url,
        string? AltText,
        bool IsMain,
        int SortOrder);
}
