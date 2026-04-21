using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Integrations;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands.PublishProduct
{
    public sealed class PublishProductCommandHandler : IRequestHandler<PublishProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductPricingAvailabilityChecker _pricingAvailabilityChecker;
        private readonly IUnitOfWork _unitOfWork;

        public PublishProductCommandHandler(
            IProductRepository productRepository,
            IProductPricingAvailabilityChecker pricingAvailabilityChecker,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _pricingAvailabilityChecker = pricingAvailabilityChecker;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PublishProductCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            var pricingTargets = BuildPricingTargets(product);
            var hasRequiredPrices = pricingTargets.Count > 0 &&
                await _pricingAvailabilityChecker.HasRequiredPricesAsync(
                    command.StoreId,
                    pricingTargets,
                    cancellationToken);

            if (!hasRequiredPrices)
                throw new ProductPricingRequiredException(command.ProductId);

            product.Publish();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static IReadOnlyCollection<ProductPricingAvailabilityTarget> BuildPricingTargets(Product product)
        {
            if (product.ProductType == ProductType.Simple)
            {
                return new[]
                {
                    new ProductPricingAvailabilityTarget(product.Id, ProductVariantId: null)
                };
            }

            return product.Variants
                .Where(x => x.IsActive)
                .Select(x => new ProductPricingAvailabilityTarget(product.Id, x.Id))
                .ToArray();
        }
    }
}
