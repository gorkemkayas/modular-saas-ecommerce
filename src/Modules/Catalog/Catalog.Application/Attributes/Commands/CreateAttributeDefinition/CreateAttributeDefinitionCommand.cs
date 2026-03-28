using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.Application.Attributes.Commands.CreateAttributeDefinition
{
    public sealed record CreateAttributeDefinitionCommand(
        Guid StoreId,
        string Name,
        string Code,
        AttributeDataType DataType,
        bool IsRequired,
        bool IsFilterable,
        bool IsVariantDefining) : IRequest<Guid>;
}
