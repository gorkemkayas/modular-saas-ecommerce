using Store.Application.Abstractions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.ActivateStore
{
    public sealed class ActivateStoreCommandHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateStoreCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ActivateStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store is null)
                throw new InvalidOperationException($"Store with Tenant ID {command.TenantId} not found.");

            store.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
