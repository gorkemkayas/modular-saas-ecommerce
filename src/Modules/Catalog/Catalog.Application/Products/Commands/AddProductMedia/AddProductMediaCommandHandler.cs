using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Products.Commands.AddProductMedia
{
    public sealed class AddProductMediaCommandHandler : IRequestHandler<AddProductMediaCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddProductMediaCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(AddProductMediaCommand command, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(command.StoreId, command.ProductId, cancellationToken);
            if (product is null)
                throw new ProductNotFoundException(command.ProductId);

            product.AddMedia(
                command.MediaType,
                command.Url,
                command.AltText,
                command.IsMain,
                command.SortOrder,
                command.ProductVariantId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
