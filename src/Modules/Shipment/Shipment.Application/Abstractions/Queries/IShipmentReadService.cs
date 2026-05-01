using Shipment.Application.Common.Models;
using Shipment.Application.Shipments.DTOs;

namespace Shipment.Application.Abstractions.Queries;

public interface IShipmentReadService
{
    Task<ShipmentDto?> GetByIdAsync(Guid storeId, Guid shipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ShipmentSummaryDto>> ListByOrderIdAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ShipmentSummaryDto>> SearchAsync(
        ShipmentSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
