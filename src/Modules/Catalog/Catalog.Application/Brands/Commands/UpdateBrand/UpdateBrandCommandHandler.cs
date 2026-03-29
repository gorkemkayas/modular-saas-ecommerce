using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Brands.Commands.UpdateBrand
{
    public sealed class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBrandCommandHandler(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateBrandCommand command, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetByIdAsync(command.StoreId, command.BrandId, cancellationToken);
            if (brand is null)
                throw new BrandNotFoundException(command.BrandId);

            var slug = Slug.Create(command.Slug);

            if (await _brandRepository.ExistsBySlugAsync(
                    command.StoreId,
                    slug,
                    command.BrandId,
                    cancellationToken))
            {
                throw new DuplicateBrandSlugException(slug.Value);
            }

            brand.Rename(command.Name);
            brand.ChangeSlug(slug);
            brand.ChangeDescription(command.Description);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
