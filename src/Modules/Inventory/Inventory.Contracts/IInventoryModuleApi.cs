namespace Inventory.Contracts;

public interface IInventoryModuleApi
{
    Task<InventoryAvailabilityResult> CheckAvailabilityAsync(
        CheckInventoryAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<InventoryReservationResult> ReserveAsync(
        ReserveInventoryRequest request,
        CancellationToken cancellationToken = default);

    Task ReleaseReservationAsync(
        ReleaseInventoryReservationRequest request,
        CancellationToken cancellationToken = default);

    Task ConfirmDeductionAsync(
        ConfirmInventoryDeductionRequest request,
        CancellationToken cancellationToken = default);
}
