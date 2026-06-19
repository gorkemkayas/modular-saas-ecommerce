using MediatR;
using Microsoft.Extensions.Logging;
using Store.Application.Abstractions;
using Store.Domain.Stores;

namespace Store.Application.Stores.Commands.DeletePendingStore;

public sealed class DeletePendingStoreCommandHandler : IRequestHandler<DeletePendingStoreCommand>
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePendingStoreCommandHandler> _logger;

    public DeletePendingStoreCommandHandler(
        IStoreRepository storeRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeletePendingStoreCommandHandler> logger)
    {
        _storeRepository = storeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeletePendingStoreCommand command, CancellationToken cancellationToken)
    {
        var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);

        if (store is null || store.Status != StoreStatus.PendingPayment)
            return;

        _storeRepository.Remove(store);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Pending store deleted after failed payment | StoreId: {StoreId} | TenantId: {TenantId}",
            store.Id,
            command.TenantId);
    }
}
