using MediatR;
using Microsoft.Extensions.Logging;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.ProvisionStoreForTenant
{
    public sealed class ProvisionStoreForTenantCommandHandler : IRequestHandler<ProvisionStoreForTenantCommand, Guid>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProvisionStoreForTenantCommandHandler> _logger;

        public ProvisionStoreForTenantCommandHandler(
            IStoreRepository storeRepository,
            IUnitOfWork unitOfWork,
            ILogger<ProvisionStoreForTenantCommandHandler> logger)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(
            ProvisionStoreForTenantCommand command,
            CancellationToken cancellationToken = default)
        {
            if (await _storeRepository.ExistsByTenantIdAsync(command.TenantId, cancellationToken))
                throw new StoreAlreadyExistsForTenantException(command.TenantId);

            var slug = Slug.Create(command.Slug);

            if (await _storeRepository.ExistsBySlugAsync(slug, cancellationToken))
                throw new DuplicateStoreSlugException(slug.Value);

            var store = Store.Domain.Stores.Store.Create(
                command.TenantId,
                command.Name,
                slug);

            await _storeRepository.AddAsync(store, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Store provisioned successfully | StoreId: {StoreId} | TenantId: {TenantId} | Name: {StoreName} | Slug: {Slug}",
                store.Id,
                command.TenantId,
                command.Name,
                slug.Value);

            return store.Id;
        }
    }
}
