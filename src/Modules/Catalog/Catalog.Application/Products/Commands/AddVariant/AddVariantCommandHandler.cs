using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.AddVariant
{
    public sealed class AddVariantCommandHandler : IRequestHandler<AddVariantCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddVariantCommandHandler> _logger;

        public AddVariantCommandHandler(
            IProductRepository productRepository,
            IAttributeDefinitionRepository attributeDefinitionRepository,
            IUnitOfWork unitOfWork,
            ILogger<AddVariantCommandHandler> logger)
        {
            _productRepository = productRepository;
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(AddVariantCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            var sku = Sku.Create(command.Sku);

            if (await _productRepository.ExistsBySkuAsync(command.StoreId, sku, cancellationToken: cancellationToken))
                throw new DuplicateProductSkuException(sku.Value);

            var attributeValues = command.AttributeValues
                .Where(x => x.AttributeDefinitionId != Guid.Empty)
                .ToArray();

            if (attributeValues.Length == 0)
                throw new CatalogValidationException("At least one variant-defining attribute is required.");

            var attributeDefinitionIds = attributeValues
                .Select(x => x.AttributeDefinitionId)
                .Distinct()
                .ToArray();

            if (attributeDefinitionIds.Length != attributeValues.Length)
                throw new CatalogValidationException("Duplicate attribute definition ids are not allowed for a single variant.");

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
                throw new CatalogValidationException($"Attribute definition '{definition.Id}' is inactive and cannot define a variant.");

            foreach (var definition in definitions.Where(x => !x.IsVariantDefining))
                throw new CatalogValidationException($"Attribute definition '{definition.Id}' is not marked as variant-defining.");

            var variant = product.AddVariant(
                sku,
                command.Name,
                command.SortOrder,
                attributeValues
                    .Select(x => (x.AttributeDefinitionId, x.Value))
                    .ToArray());

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Catalog product variant added | ProductId: {ProductId} | VariantId: {VariantId} | StoreId: {StoreId}",
                command.ProductId,
                variant.Id,
                command.StoreId);

            return variant.Id;
        }
    }
}
