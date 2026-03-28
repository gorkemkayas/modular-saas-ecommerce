using Catalog.Application.Attributes.DTOs;
using MediatR;

namespace Catalog.Application.Attributes.Queries.ListAttributeDefinitions
{
    public sealed record ListAttributeDefinitionsQuery(Guid StoreId, bool ActiveOnly = false) : IRequest<IReadOnlyCollection<AttributeDefinitionDto>>;
}
