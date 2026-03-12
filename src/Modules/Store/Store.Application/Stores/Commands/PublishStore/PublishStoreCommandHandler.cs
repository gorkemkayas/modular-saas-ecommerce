using MediatR;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.PublishStore
{
    public sealed class PublishStoreCommandHandler : IRequestHandler<PublishStoreCommand>
    {
        private readonly IStoreRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public PublishStoreCommandHandler(IStoreRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(PublishStoreCommand command, CancellationToken cancellationToken)
        {
            var store = await _repository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store == null)
                throw new StoreNotFoundException(command.TenantId);

            store.Publish();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
