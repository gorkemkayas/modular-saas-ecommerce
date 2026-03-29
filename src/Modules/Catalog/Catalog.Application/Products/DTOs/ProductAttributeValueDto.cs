namespace Catalog.Application.Products.DTOs
{
    public sealed record ProductAttributeValueDto(
        Guid AttributeDefinitionId,
        Guid? ProductId,
        Guid? ProductVariantId,
        string Value);
}
