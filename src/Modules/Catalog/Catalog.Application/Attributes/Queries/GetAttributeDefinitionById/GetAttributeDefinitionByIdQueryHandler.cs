using Catalog.Application.Abstractions.Queries;
using Catalog.Application.Attributes.DTOs;
using MediatR;

namespace Catalog.Application.Attributes.Queries.GetAttributeDefinitionById
{
    public sealed class GetAttributeDefinitionByIdQueryHandler : IRequestHandler<GetAttributeDefinitionByIdQuery, AttributeDefinitionDto?>
    {
        private readonly IAttributeDefinitionReadService _attributeDefinitionReadService;

        public GetAttributeDefinitionByIdQueryHandler(IAttributeDefinitionReadService attributeDefinitionReadService)
        {
            _attributeDefinitionReadService = attributeDefinitionReadService;
        }

        public Task<AttributeDefinitionDto?> Handle(GetAttributeDefinitionByIdQuery query, CancellationToken cancellationToken)
        {
            return _attributeDefinitionReadService.GetByIdAsync(query.StoreId, query.AttributeDefinitionId, cancellationToken);
        }
    }
}
