using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Brands.Commands.ActivateBrand
{
    public sealed class ActivateBrandCommandHandler : IRequestHandler<ActivateBrandCommand>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateBrandCommandHandler(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ActivateBrandCommand command, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(command.StoreId, command.BrandId, cancellationToken);
            if (brand is null)
                throw new BrandNotFoundException(command.BrandId);

            brand.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
