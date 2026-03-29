using Catalog.Application.Attributes.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Attributes
{
    internal static class AttributeDefinitionMappings
    {
        public static AttributeDefinitionDto ToDto(this AttributeDefinition attributeDefinition)
        {
            return new AttributeDefinitionDto(
                attributeDefinition.Id,
                attributeDefinition.StoreId,
                attributeDefinition.Name,
                attributeDefinition.Code.Value,
                attributeDefinition.DataType,
                attributeDefinition.IsRequired,
                attributeDefinition.IsFilterable,
                attributeDefinition.IsVariantDefining,
                attributeDefinition.IsActive,
                attributeDefinition.CreatedAtUtc,
                attributeDefinition.UpdatedAtUtc);
        }
    }
}
