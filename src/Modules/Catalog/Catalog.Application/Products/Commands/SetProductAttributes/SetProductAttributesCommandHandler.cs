using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands.SetProductAttributes
{
    public sealed class SetProductAttributesCommandHandler : IRequestHandler<SetProductAttributesCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetProductAttributesCommandHandler(
            IProductRepository productRepository,
            IAttributeDefinitionRepository attributeDefinitionRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SetProductAttributesCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            var attributeValues = command.AttributeValues
                .Where(x => x.AttributeDefinitionId != Guid.Empty)
                .ToArray();

            var attributeDefinitionIds = attributeValues
                .Select(x => x.AttributeDefinitionId)
                .Distinct()
                .ToArray();

            if (attributeDefinitionIds.Length != attributeValues.Length)
                throw new CatalogValidationException("Duplicate attribute definition ids are not allowed in the same request.");

            var definitions = await _attributeDefinitionRepository.GetByIdsAsync(
                command.StoreId,
                attributeDefinitionIds,
                cancellationToken);

            if (definitions.Count != attributeDefinitionIds.Length)
            {
                var missingId = attributeDefinitionIds.First(id => definitions.All(x => x.Id != id));
                throw new AttributeDefinitionNotFoundException(missingId);
            }

            foreach (var definition in definitions.Where(x => !x.IsActive))
                throw new CatalogValidationException($"Attribute definition '{definition.Id}' is inactive and cannot be assigned.");

            foreach (var attributeValue in attributeValues)
                product.SetProductAttributeValue(attributeValue.AttributeDefinitionId, attributeValue.Value);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
