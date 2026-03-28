using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Attributes.DTOs;
using MediatR;

namespace Catalog.Application.Attributes.Queries.ListAttributeDefinitions
{
    public sealed class ListAttributeDefinitionsQueryHandler : IRequestHandler<ListAttributeDefinitionsQuery, IReadOnlyCollection<AttributeDefinitionDto>>
    {
        private readonly IAttributeDefinitionReadService _attributeDefinitionReadService;

        public ListAttributeDefinitionsQueryHandler(IAttributeDefinitionReadService attributeDefinitionReadService)
        {
            _attributeDefinitionReadService = attributeDefinitionReadService;
        }

        public Task<IReadOnlyCollection<AttributeDefinitionDto>> Handle(ListAttributeDefinitionsQuery query, CancellationToken cancellationToken)
        {
            return _attributeDefinitionReadService.ListByStoreAsync(query.StoreId, query.ActiveOnly, cancellationToken);
        }
    }
}
