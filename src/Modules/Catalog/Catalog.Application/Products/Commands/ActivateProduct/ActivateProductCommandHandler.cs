using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands.ActivateProduct
{
    public sealed class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ActivateProductCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            product.Activate();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
