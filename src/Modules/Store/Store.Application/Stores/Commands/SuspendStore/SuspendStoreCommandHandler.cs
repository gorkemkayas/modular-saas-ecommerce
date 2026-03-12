using MediatR;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.SuspendStore
{
    public sealed class SuspendStoreCommandHandler : IRequestHandler<SuspendStoreCommand>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SuspendStoreCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SuspendStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store is null)
                throw new StoreNotFoundException(command.TenantId);

            store.Suspend();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
