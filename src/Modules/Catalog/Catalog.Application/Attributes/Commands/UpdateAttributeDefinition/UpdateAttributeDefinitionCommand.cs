using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.Application.Attributes.Commands.UpdateAttributeDefinition
{
    public sealed record UpdateAttributeDefinitionCommand(
        Guid StoreId,
        Guid AttributeDefinitionId,
        string Name,
        string Code,
        AttributeDataType DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantDefining) : IRequest;
}
