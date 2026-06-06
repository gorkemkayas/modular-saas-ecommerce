using Shipment.Domain.Entities;

namespace Shipment.Domain.Repositories;

public interface IShippingCarrierRepository
{
    Task AddAsync(ShippingCarrier carrier, CancellationToken cancellationToken = default);

    Task<ShippingCarrier?> GetByIdAsync(
        Guid storeId,
        Guid carrierId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        Guid storeId,
        string code,
        Guid? excludedCarrierId = null,
        CancellationToken cancellationToken = default);
}
