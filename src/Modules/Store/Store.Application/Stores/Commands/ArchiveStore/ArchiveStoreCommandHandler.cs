using Store.Application.Abstractions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.ArchiveStore
{
    public sealed class ArchiveStoreCommandHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ArchiveStoreCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ArchiveStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store is null)
                throw new InvalidOperationException("Store not found.");

            store.Archive();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
