using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands.AssignProductCategories
{
    public sealed class AssignProductCategoriesCommandHandler : IRequestHandler<AssignProductCategoriesCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignProductCategoriesCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(AssignProductCategoriesCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            var categoryIds = command.CategoryIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();

            foreach (var categoryId in categoryIds)
            {
                if (!await _categoryRepository.ExistsByIdAsync(command.StoreId, categoryId, cancellationToken))
                    throw new CategoryNotFoundException(categoryId);
            }

            product.AssignCategories(categoryIds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
