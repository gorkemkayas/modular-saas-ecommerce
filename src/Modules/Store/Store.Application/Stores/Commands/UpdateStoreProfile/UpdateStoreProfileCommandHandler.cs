using MediatR;
using Store.Application.Abstractions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.UpdateStoreProfile
{
    public sealed class UpdateStoreProfileCommandHandler : IRequestHandler<UpdateStoreProfileCommand>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStoreProfileCommandHandler(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateStoreProfileCommand command, CancellationToken cancellationToken = default)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store == null)
            {
                throw new InvalidOperationException($"Store with TenantId {command.TenantId} not found.");
            }

            store.UpdateProfile(command.Name, command.Description, command.LogoUrl);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
