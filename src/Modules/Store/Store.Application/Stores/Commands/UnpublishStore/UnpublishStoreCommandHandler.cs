using Store.Application.Abstractions;
using Store.Application.Stores.Commands.PublishStore;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.UnpublishStore
{
    public sealed class UnpublishStoreCommandHandler
    {
        private readonly IStoreRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UnpublishStoreCommandHandler(IStoreRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PublishStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _repository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store == null)
                throw new InvalidOperationException("Store not found.");

            store.Unpublish();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
