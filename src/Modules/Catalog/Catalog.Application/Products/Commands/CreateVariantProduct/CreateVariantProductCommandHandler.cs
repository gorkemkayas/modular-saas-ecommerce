using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Application.Products;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Subscription.Contracts;

namespace Catalog.Application.Products.Commands.CreateVariantProduct
{
    public sealed class CreateVariantProductCommandHandler : IRequestHandler<CreateVariantProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubscriptionModuleApi _subscriptionModuleApi;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateVariantProductCommandHandler> _logger;

        public CreateVariantProductCommandHandler(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            ISubscriptionModuleApi subscriptionModuleApi,
            IUnitOfWork unitOfWork,
            ILogger<CreateVariantProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _subscriptionModuleApi = subscriptionModuleApi;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateVariantProductCommand command, CancellationToken cancellationToken)
        {
            if (command.StoreId == Guid.Empty)
                throw new CatalogValidationException("StoreId is required.");

            await CatalogSubscriptionGuard.EnsureCanCreateVariantProductAsync(
                command.StoreId,
                _subscriptionModuleApi,
                cancellationToken);

            var slug = Slug.Create(command.Slug);

            if (await _productRepository.ExistsBySlugAsync(command.StoreId, slug, cancellationToken: cancellationToken))
                throw new DuplicateProductSlugException(slug.Value);

            if (command.BrandId.HasValue &&
                !await _brandRepository.ExistsByIdAsync(command.StoreId, command.BrandId.Value, cancellationToken))
            {
                throw new BrandNotFoundException(command.BrandId.Value);
            }

            var categoryIds = command.CategoryIds?
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray() ?? Array.Empty<Guid>();

            foreach (var categoryId in categoryIds)
            {
                if (!await _categoryRepository.ExistsByIdAsync(command.StoreId, categoryId, cancellationToken))
                    throw new CategoryNotFoundException(categoryId);
            }

            await CatalogSubscriptionGuard.EnsureCanCreateProductAsync(
                command.StoreId,
                _productRepository,
                _subscriptionModuleApi,
                cancellationToken);

            var product = Product.CreateVariant(
                command.StoreId,
                command.Name,
                slug,
                command.ShortDescription,
                command.Description,
                command.BrandId);

            if (categoryIds.Length > 0)
                product.AssignCategories(categoryIds);

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Catalog product created | ProductId: {ProductId} | StoreId: {StoreId} | ProductType: {ProductType}",
                product.Id,
                command.StoreId,
                product.ProductType);

            return product.Id;
        }
    }
}
