using MediatR;

namespace Catalog.Application.Attributes.Commands.ActivateAttributeDefinition
{
    public sealed record ActivateAttributeDefinitionCommand(Guid StoreId, Guid AttributeDefinitionId) : IRequest;
}
