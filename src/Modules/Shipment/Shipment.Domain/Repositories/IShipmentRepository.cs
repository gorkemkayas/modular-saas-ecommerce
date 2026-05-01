using ShipmentEntity = Shipment.Domain.Entities.Shipment;

namespace Shipment.Domain.Repositories;

public interface IShipmentRepository
{
    Task AddAsync(ShipmentEntity shipment, CancellationToken cancellationToken = default);
    Task<ShipmentEntity?> GetByIdAsync(Guid storeId, Guid shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentEntity?> GetActiveForOrderAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ShipmentEntity>> ListByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveForOrderAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);
}
