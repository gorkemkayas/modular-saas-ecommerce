using Microsoft.EntityFrameworkCore;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.ShippingCarriers.DTOs;
using Shipment.Infrastructure.Persistence;

namespace Shipment.Infrastructure.ReadServices;

public sealed class ShippingCarrierReadService : IShippingCarrierReadService
{
    private readonly ShipmentDbContext _context;

    public ShippingCarrierReadService(ShipmentDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ShippingCarrierDto>> ListAsync(
        Guid storeId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ShippingCarriers
            .AsNoTracking()
            .Where(x => x.StoreId == storeId);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ShippingCarrierDto(
                x.Id,
                x.StoreId,
                x.Code,
                x.Name,
                x.ServiceCode,
                x.ServiceName,
                x.TrackingUrl,
                x.IsActive,
                x.SortOrder,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public Task<ShippingCarrierDto?> GetActiveByIdAsync(
        Guid storeId,
        Guid carrierId,
        CancellationToken cancellationToken = default)
    {
        return _context.ShippingCarriers
            .AsNoTracking()
            .Where(x => x.StoreId == storeId && x.Id == carrierId && x.IsActive)
            .Select(x => new ShippingCarrierDto(
                x.Id,
                x.StoreId,
                x.Code,
                x.Name,
                x.ServiceCode,
                x.ServiceName,
                x.TrackingUrl,
                x.IsActive,
                x.SortOrder,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
