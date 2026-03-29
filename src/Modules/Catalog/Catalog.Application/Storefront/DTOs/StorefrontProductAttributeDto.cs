namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontProductAttributeDto(
        Guid AttributeDefinitionId,
        string Name,
        string Code,
        string Value,
        bool IsVariantDefining);
}
