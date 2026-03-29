using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Products.Commands.CreateSimpleProduct
{
    public sealed class CreateSimpleProductCommandHandler : IRequestHandler<CreateSimpleProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateSimpleProductCommandHandler> _logger;

        public CreateSimpleProductCommandHandler(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateSimpleProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateSimpleProductCommand command, CancellationToken cancellationToken)
        {
            EnsureStoreId(command.StoreId);

            var slug = Slug.Create(command.Slug);
            var sku = Sku.Create(command.Sku);

            if (await _productRepository.ExistsBySlugAsync(command.StoreId, slug, cancellationToken: cancellationToken))
                throw new DuplicateProductSlugException(slug.Value);

            if (await _productRepository.ExistsBySkuAsync(command.StoreId, sku, cancellationToken: cancellationToken))
                throw new DuplicateProductSkuException(sku.Value);

            if (command.BrandId.HasValue &&
                !await _brandRepository.ExistsByIdAsync(command.StoreId, command.BrandId.Value, cancellationToken))
            {
                throw new BrandNotFoundException(command.BrandId.Value);
            }

            var categoryIds = NormalizeIds(command.CategoryIds);
            await EnsureCategoriesExistAsync(command.StoreId, categoryIds, cancellationToken);

            var product = Product.CreateSimple(
                command.StoreId,
                command.Name,
                slug,
                sku,
                command.ShortDescription,
                command.Description,
                command.BrandId);

            if (categoryIds.Count > 0)
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

        private static void EnsureStoreId(Guid storeId)
        {
            if (storeId == Guid.Empty)
                throw new CatalogValidationException("StoreId is required.");
        }

        private static IReadOnlyCollection<Guid> NormalizeIds(IReadOnlyCollection<Guid>? ids)
        {
            return ids?
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray() ?? Array.Empty<Guid>();
        }

        private async Task EnsureCategoriesExistAsync(
            Guid storeId,
            IReadOnlyCollection<Guid> categoryIds,
            CancellationToken cancellationToken)
        {
            foreach (var categoryId in categoryIds)
            {
                if (!await _categoryRepository.ExistsByIdAsync(storeId, categoryId, cancellationToken))
                    throw new CategoryNotFoundException(categoryId);
            }
        }
    }
}
