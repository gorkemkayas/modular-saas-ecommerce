namespace Catalog.Application.Storefront.DTOs
{
    public sealed record StorefrontAttributeFacetDto(
        Guid AttributeDefinitionId,
        string Name,
        string Code,
        IReadOnlyCollection<StorefrontFacetValueDto> Values);
}
