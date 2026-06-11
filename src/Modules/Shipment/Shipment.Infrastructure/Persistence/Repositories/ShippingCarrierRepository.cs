using Microsoft.EntityFrameworkCore;
using Shipment.Domain.Entities;
using Shipment.Domain.Repositories;

namespace Shipment.Infrastructure.Persistence.Repositories;

public sealed class ShippingCarrierRepository : IShippingCarrierRepository
{
    private readonly ShipmentDbContext _context;

    public ShippingCarrierRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ShippingCarrier carrier, CancellationToken cancellationToken = default)
    {
        return _context.ShippingCarriers.AddAsync(carrier, cancellationToken).AsTask();
    }

    public Task<ShippingCarrier?> GetByIdAsync(
        Guid storeId,
        Guid carrierId,
        CancellationToken cancellationToken = default)
    {
        return _context.ShippingCarriers
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == carrierId, cancellationToken);
    }

    public Task<int> CountActiveByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        return _context.ShippingCarriers.CountAsync(
            x => x.StoreId == storeId && x.IsActive,
            cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(
        Guid storeId,
        string code,
        Guid? excludedCarrierId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ShippingCarriers
            .Where(x => x.StoreId == storeId && x.Code == code);

        if (excludedCarrierId.HasValue)
            query = query.Where(x => x.Id != excludedCarrierId.Value);

        return query.AnyAsync(cancellationToken);
    }
}
