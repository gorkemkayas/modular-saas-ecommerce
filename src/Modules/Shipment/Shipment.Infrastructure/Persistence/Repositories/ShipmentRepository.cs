using Microsoft.EntityFrameworkCore;
using Shipment.Domain.Repositories;
using Shipment.Infrastructure.Persistence;
using ShipmentEntity = Shipment.Domain.Entities.Shipment;

namespace Shipment.Infrastructure.Persistence.Repositories;

public sealed class ShipmentRepository : IShipmentRepository
{
    private readonly ShipmentDbContext _context;

    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ShipmentEntity shipment, CancellationToken cancellationToken = default)
    {
        return _context.Shipments.AddAsync(shipment, cancellationToken).AsTask();
    }

    public Task<ShipmentEntity?> GetByIdAsync(Guid storeId, Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return _context.Shipments
            .Include(x => x.Lines)
            .Include(x => x.Packages)
                .ThenInclude(x => x.TrackingEvents)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == shipmentId, cancellationToken);
    }

    public Task<ShipmentEntity?> GetActiveForOrderAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.Shipments
            .Include(x => x.Lines)
            .Include(x => x.Packages)
                .ThenInclude(x => x.TrackingEvents)
            .Where(x => x.StoreId == storeId
                && x.OrderId == orderId
                && x.Status != Domain.Enums.ShipmentStatus.Cancelled)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ShipmentEntity>> ListByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(x => x.Lines)
            .Include(x => x.Packages)
                .ThenInclude(x => x.TrackingEvents)
            .Where(x => x.StoreId == storeId && x.OrderId == orderId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveForOrderAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.Shipments.AnyAsync(
            x => x.StoreId == storeId
                && x.OrderId == orderId
                && x.Status != Domain.Enums.ShipmentStatus.Cancelled,
            cancellationToken);
    }
}
