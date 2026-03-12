using MediatR;
using Microsoft.Extensions.Logging;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.ArchiveStore
{
    public sealed class ArchiveStoreCommandHandler : IRequestHandler<ArchiveStoreCommand>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ArchiveStoreCommandHandler> _logger;

        public ArchiveStoreCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork, ILogger<ArchiveStoreCommandHandler> logger)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(ArchiveStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store is null)
                throw new StoreNotFoundException(command.TenantId);

            store.Archive();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Store provisioned successfully | StoreId: {StoreId} | TenantId: {TenantId} | Name: {StoreName} | Slug: {Slug}",
                store.Id,
                command.TenantId,
                store.Name,
                store.Slug);
        }
    }
}
