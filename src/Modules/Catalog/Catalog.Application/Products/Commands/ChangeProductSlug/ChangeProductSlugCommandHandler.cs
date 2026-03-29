using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Products.Commands.ChangeProductSlug
{
    public sealed class ChangeProductSlugCommandHandler : IRequestHandler<ChangeProductSlugCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeProductSlugCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ChangeProductSlugCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            var slug = Slug.Create(command.Slug);

            if (await _productRepository.ExistsBySlugAsync(
                    command.StoreId,
                    slug,
                    command.ProductId,
                    cancellationToken))
            {
                throw new DuplicateProductSlugException(slug.Value);
            }

            product.ChangeSlug(slug);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
