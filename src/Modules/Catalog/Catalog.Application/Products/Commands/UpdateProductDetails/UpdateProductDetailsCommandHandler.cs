using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands.UpdateProductDetails
{
    public sealed class UpdateProductDetailsCommandHandler : IRequestHandler<UpdateProductDetailsCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductDetailsCommandHandler(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateProductDetailsCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            if (command.BrandId.HasValue &&
                !await _brandRepository.ExistsByIdAsync(command.StoreId, command.BrandId.Value, cancellationToken))
            {
                throw new BrandNotFoundException(command.BrandId.Value);
            }

            product.UpdateDetails(command.Name, command.ShortDescription, command.Description);
            product.ChangeBrand(command.BrandId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
