using Catalog.Application.Attributes.DTOs;
using MediatR;

namespace Catalog.Application.Attributes.Queries.GetAttributeDefinitionById
{
    public sealed record GetAttributeDefinitionByIdQuery(Guid StoreId, Guid AttributeDefinitionId) : IRequest<AttributeDefinitionDto?>;
}
