
using MediatR;
using Store.Application.Abstractions;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.ProvisionStoreForTenant
{
    public sealed class ProvisionStoreForTenantCommandHandler : IRequestHandler<ProvisionStoreForTenantCommand, Guid>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProvisionStoreForTenantCommandHandler(
            IStoreRepository storeRepository,
            IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(
            ProvisionStoreForTenantCommand command,
            CancellationToken cancellationToken = default)
        {
            if (await _storeRepository.ExistsByTenantIdAsync(command.TenantId, cancellationToken))
                throw new InvalidOperationException("A store already exists for this tenant.");

            var slug = Slug.Create(command.Slug);

            if (await _storeRepository.ExistsBySlugAsync(slug, cancellationToken))
                throw new InvalidOperationException("Slug is already in use.");

            var store = Store.Domain.Stores.Store.Create(
                command.TenantId,
                command.Name,
                slug);

            await _storeRepository.AddAsync(store, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return store.Id;
        }
    }
}
