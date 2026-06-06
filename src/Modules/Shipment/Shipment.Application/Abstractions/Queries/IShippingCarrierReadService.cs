using Shipment.Application.ShippingCarriers.DTOs;

namespace Shipment.Application.Abstractions.Queries;

public interface IShippingCarrierReadService
{
    Task<IReadOnlyCollection<ShippingCarrierDto>> ListAsync(
        Guid storeId,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task<ShippingCarrierDto?> GetActiveByIdAsync(
        Guid storeId,
        Guid carrierId,
        CancellationToken cancellationToken = default);
}
