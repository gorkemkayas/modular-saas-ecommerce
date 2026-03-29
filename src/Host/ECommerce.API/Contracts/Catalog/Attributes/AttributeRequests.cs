using Catalog.Domain.Enums;

namespace ECommerce.API.Contracts.Catalog.Attributes
{
    public sealed record ListAttributeDefinitionsRequest(bool ActiveOnly = false);

    public sealed record CreateAttributeDefinitionRequest(
        string Name,
        string Code,
        AttributeDataType DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantDefining);

    public sealed record UpdateAttributeDefinitionRequest(
        string Name,
        string Code,
        AttributeDataType DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantDefining);
}
