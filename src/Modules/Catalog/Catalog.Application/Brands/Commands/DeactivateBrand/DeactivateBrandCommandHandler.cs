using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Brands.Commands.DeactivateBrand
{
    public sealed class DeactivateBrandCommandHandler : IRequestHandler<DeactivateBrandCommand>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateBrandCommandHandler(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeactivateBrandCommand command, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(command.StoreId, command.BrandId, cancellationToken);
            if (brand is null)
                throw new BrandNotFoundException(command.BrandId);

            brand.Deactivate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
