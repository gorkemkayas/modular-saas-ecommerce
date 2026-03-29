using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Brands.Commands.CreateBrand
{
    public sealed class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Guid>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateBrandCommandHandler> _logger;

        public CreateBrandCommandHandler(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateBrandCommandHandler> logger)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateBrandCommand command, CancellationToken cancellationToken)
        {
            if (command.StoreId == Guid.Empty)
                throw new CatalogValidationException("StoreId is required.");

            var slug = Slug.Create(command.Slug);

            if (await _brandRepository.ExistsBySlugAsync(command.StoreId, slug, cancellationToken: cancellationToken))
                throw new DuplicateBrandSlugException(slug.Value);

            var brand = Brand.Create(command.StoreId, command.Name, slug, command.Description);

            await _brandRepository.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Catalog brand created | BrandId: {BrandId} | StoreId: {StoreId}",
                brand.Id,
                brand.StoreId);

            return brand.Id;
        }
    }
}
