namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductVariantDto(
        Guid Id,
        Guid ProductId,
        string Sku,
        string? Name,
        bool IsActive,
        int SortOrder,
        IReadOnlyCollection<ProductAttributeValueDto> AttributeValues);
}
