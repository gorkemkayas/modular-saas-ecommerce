using MediatR;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.UnpublishStore
{
    public sealed class UnpublishStoreCommandHandler : IRequestHandler<UnpublishStoreCommand>
    {
        private readonly IStoreRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UnpublishStoreCommandHandler(IStoreRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UnpublishStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _repository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store == null)
                throw new StoreNotFoundException(command.TenantId);

            store.Unpublish();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
