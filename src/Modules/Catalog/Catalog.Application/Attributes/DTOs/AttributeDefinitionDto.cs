using Catalog.Domain.Enums;

namespace Catalog.Application.Attributes.DTOs
{
    public sealed record AttributeDefinitionDto(
        Guid Id,
        Guid StoreId,
        string Name,
        string Code,
        AttributeDataType DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantDefining,
        bool IsActive,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
