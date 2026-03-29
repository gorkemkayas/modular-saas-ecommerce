using MediatR;

namespace Catalog.Application.Attributes.Commands.DeactivateAttributeDefinition
{
    public sealed record DeactivateAttributeDefinitionCommand(Guid StoreId, Guid AttributeDefinitionId) : IRequest;
}
