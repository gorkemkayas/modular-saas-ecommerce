using Inventory.Contracts;
using Payment.Application.Integrations;

namespace Payment.Infrastructure.Integrations.Inventory;

public sealed class PaymentInventoryService : IInventoryPaymentService
{
    private readonly IInventoryModuleApi _inventoryModuleApi;

    public PaymentInventoryService(IInventoryModuleApi inventoryModuleApi)
    {
        _inventoryModuleApi = inventoryModuleApi;
    }

    public Task ConfirmDeductionAsync(
        Guid storeId,
        string reservationReference,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return _inventoryModuleApi.ConfirmDeductionAsync(
            new ConfirmInventoryDeductionRequest(storeId, reservationReference, reason),
            cancellationToken);
    }

    public Task ReleaseReservationAsync(
        Guid storeId,
        string reservationReference,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return _inventoryModuleApi.ReleaseReservationAsync(
            new ReleaseInventoryReservationRequest(storeId, reservationReference, reason),
            cancellationToken);
    }
}
