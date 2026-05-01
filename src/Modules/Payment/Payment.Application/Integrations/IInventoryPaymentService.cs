namespace Payment.Application.Integrations;

public interface IInventoryPaymentService
{
    Task ConfirmDeductionAsync(Guid storeId, string reservationReference, string reason, CancellationToken cancellationToken = default);
    Task ReleaseReservationAsync(Guid storeId, string reservationReference, string reason, CancellationToken cancellationToken = default);
}
