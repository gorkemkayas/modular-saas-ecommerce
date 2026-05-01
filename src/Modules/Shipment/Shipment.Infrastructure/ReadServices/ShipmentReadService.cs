using Microsoft.EntityFrameworkCore;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Common.Models;
using Shipment.Application.Shipments.DTOs;
using Shipment.Infrastructure.Persistence;

namespace Shipment.Infrastructure.ReadServices;

public sealed class ShipmentReadService : IShipmentReadService
{
    private readonly ShipmentDbContext _context;

    public ShipmentReadService(ShipmentDbContext context)
    {
        _context = context;
    }

    public async Task<ShipmentDto?> GetByIdAsync(Guid storeId, Guid shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _context.Shipments
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Packages)
                .ThenInclude(x => x.TrackingEvents)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == shipmentId, cancellationToken);

        return shipment is null ? null : Map(shipment);
    }

    public async Task<IReadOnlyCollection<ShipmentSummaryDto>> ListByOrderIdAsync(
        Guid storeId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .AsNoTracking()
            .Include(x => x.Packages)
            .Where(x => x.StoreId == storeId && x.OrderId == orderId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ShipmentSummaryDto(
                x.Id,
                x.OrderId,
                x.OrderNumber,
                x.ShipmentNumber,
                x.Status,
                x.RecipientName,
                x.CarrierName,
                x.Packages
                    .OrderBy(p => p.CreatedAtUtc)
                    .Select(p => p.TrackingNumber)
                    .Where(number => number != null)
                    .FirstOrDefault(),
                x.CreatedAtUtc,
                x.ShippedAtUtc,
                x.DeliveredAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<ShipmentSummaryDto>> SearchAsync(
        ShipmentSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Shipments
            .AsNoTracking()
            .Include(x => x.Packages)
            .Where(x => x.StoreId == criteria.StoreId);

        if (criteria.Status.HasValue)
            query = query.Where(x => x.Status == criteria.Status.Value);

        if (criteria.OrderId.HasValue)
            query = query.Where(x => x.OrderId == criteria.OrderId.Value);

        if (!string.IsNullOrWhiteSpace(criteria.OrderNumber))
        {
            var orderNumber = criteria.OrderNumber.Trim();
            query = query.Where(x => x.OrderNumber.Contains(orderNumber));
        }

        if (!string.IsNullOrWhiteSpace(criteria.ShipmentNumber))
        {
            var shipmentNumber = criteria.ShipmentNumber.Trim();
            query = query.Where(x => x.ShipmentNumber.Contains(shipmentNumber));
        }

        if (!string.IsNullOrWhiteSpace(criteria.TrackingNumber))
        {
            var trackingNumber = criteria.TrackingNumber.Trim();
            query = query.Where(x => x.Packages.Any(p => p.TrackingNumber != null && p.TrackingNumber.Contains(trackingNumber)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(x => new ShipmentSummaryDto(
                x.Id,
                x.OrderId,
                x.OrderNumber,
                x.ShipmentNumber,
                x.Status,
                x.RecipientName,
                x.CarrierName,
                x.Packages
                    .OrderBy(p => p.CreatedAtUtc)
                    .Select(p => p.TrackingNumber)
                    .Where(number => number != null)
                    .FirstOrDefault(),
                x.CreatedAtUtc,
                x.ShippedAtUtc,
                x.DeliveredAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<ShipmentSummaryDto>(items, criteria.PageNumber, criteria.PageSize, totalCount);
    }

    private static ShipmentDto Map(Shipment.Domain.Entities.Shipment shipment)
    {
        return new ShipmentDto(
            shipment.Id,
            shipment.StoreId,
            shipment.OrderId,
            shipment.OrderNumber,
            shipment.ShipmentNumber,
            shipment.Status,
            shipment.RecipientName,
            shipment.RecipientPhoneNumber,
            new ShipmentAddressDto(
                shipment.DestinationAddress.ContactName,
                shipment.DestinationAddress.PhoneNumber,
                shipment.DestinationAddress.Country,
                shipment.DestinationAddress.City,
                shipment.DestinationAddress.District,
                shipment.DestinationAddress.Line1,
                shipment.DestinationAddress.Line2,
                shipment.DestinationAddress.PostalCode),
            shipment.CarrierCode,
            shipment.CarrierName,
            shipment.ServiceCode,
            shipment.ServiceName,
            shipment.TrackingUrl,
            shipment.InternalNote,
            shipment.CancellationReason,
            shipment.CreatedAtUtc,
            shipment.UpdatedAtUtc,
            shipment.ReadyForDispatchAtUtc,
            shipment.ShippedAtUtc,
            shipment.DeliveredAtUtc,
            shipment.CancelledAtUtc,
            shipment.Lines
                .Select(line => new ShipmentLineDto(
                    line.Id,
                    line.OrderItemId,
                    line.ProductId,
                    line.ProductVariantId,
                    line.ProductName,
                    line.VariantName,
                    line.Sku,
                    line.Quantity))
                .ToArray(),
            shipment.Packages
                .OrderBy(x => x.CreatedAtUtc)
                .Select(package => new ShipmentPackageDto(
                    package.Id,
                    package.PackageNumber,
                    package.TrackingNumber,
                    package.Weight,
                    package.WeightUnit,
                    package.LabelReference,
                    package.CreatedAtUtc,
                    package.ShippedAtUtc,
                    package.TrackingEvents
                        .OrderBy(x => x.OccurredAtUtc)
                        .Select(e => new TrackingEventDto(
                            e.Id,
                            e.Type,
                            e.OccurredAtUtc,
                            e.Location,
                            e.Description,
                            e.RawStatusCode,
                            e.RawStatusText))
                        .ToArray()))
                .ToArray());
    }
}
