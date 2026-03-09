using Store.Application.Abstractions;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Commands.ChangeStoreSlug
{
    public sealed class ChangeStoreSlugCommandHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeStoreSlugCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;

        }

        public async Task Handle(ChangeStoreSlugCommand command, CancellationToken cancellationToken = default)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store is null)
                    throw new InvalidOperationException("Store not found for the given tenant.");

            var newSlug = Slug.Create(command.NewSlug);

            var isExist = await _storeRepository.ExistsBySlugAsync(newSlug, cancellationToken);
            if (isExist)
                throw new InvalidOperationException("The provided slug is already in use by another store.");

            store.ChangeSlug(newSlug);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
    }
}
