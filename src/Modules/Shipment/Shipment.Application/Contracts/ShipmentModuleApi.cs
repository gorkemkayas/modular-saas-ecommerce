using MediatR;
using Shipment.Application.Abstractions.Queries;
using Shipment.Application.Shipments.Commands.EnsureShipmentCreatedForCapturedOrder;
using Shipment.Application.ShippingCarriers.DTOs;
using Shipment.Contracts;

namespace Shipment.Application.Contracts;

public sealed class ShipmentModuleApi : IShipmentModuleApi
{
    private readonly ISender _sender;
    private readonly IShipmentReadService _shipmentReadService;
    private readonly IShippingCarrierReadService _shippingCarrierReadService;

    public ShipmentModuleApi(
        ISender sender,
        IShipmentReadService shipmentReadService,
        IShippingCarrierReadService shippingCarrierReadService)
    {
        _sender = sender;
        _shipmentReadService = shipmentReadService;
        _shippingCarrierReadService = shippingCarrierReadService;
    }

    public Task<Guid> EnsureShipmentCreatedForCapturedOrderAsync(
        EnsureShipmentCreatedForCapturedOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        return _sender.Send(
            new EnsureShipmentCreatedForCapturedOrderCommand(
                request.StoreId,
                request.OrderId,
                request.InternalNote),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ShipmentSummaryResult>> GetStoreOrderShipmentsAsync(
        GetStoreOrderShipmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var shipments = await _shipmentReadService.ListByOrderIdAsync(
            request.StoreId,
            request.OrderId,
            cancellationToken);

        return shipments
            .Select(shipment => new ShipmentSummaryResult(
                shipment.Id,
                shipment.OrderId,
                shipment.OrderNumber,
                shipment.ShipmentNumber,
                shipment.Status,
                shipment.RecipientName,
                shipment.CarrierName,
                shipment.TrackingNumber,
                shipment.CreatedAtUtc,
                shipment.ShippedAtUtc,
                shipment.DeliveredAtUtc))
            .ToArray();
    }

    public async Task<ShipmentDetailResult?> GetStoreShipmentByIdAsync(
        GetStoreShipmentByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentReadService.GetByIdAsync(request.StoreId, request.ShipmentId, cancellationToken);

        return shipment is null
            ? null
            : new ShipmentDetailResult(
                shipment.Id,
                shipment.StoreId,
                shipment.OrderId,
                shipment.OrderNumber,
                shipment.ShipmentNumber,
                shipment.Status,
                shipment.RecipientName,
                shipment.RecipientPhoneNumber,
                new ShipmentAddressResult(
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
                    .Select(line => new ShipmentLineResult(
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
                    .Select(package => new ShipmentPackageResult(
                        package.Id,
                        package.PackageNumber,
                        package.TrackingNumber,
                        package.Weight,
                        package.WeightUnit,
                        package.LabelReference,
                        package.CreatedAtUtc,
                        package.ShippedAtUtc,
                        package.TrackingEvents
                            .Select(e => new ShipmentTrackingEventResult(
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

    public async Task<IReadOnlyCollection<ShippingCarrierResult>> GetActiveShippingCarriersAsync(
        GetActiveShippingCarriersRequest request,
        CancellationToken cancellationToken = default)
    {
        var carriers = await _shippingCarrierReadService.ListAsync(
            request.StoreId,
            activeOnly: true,
            cancellationToken);

        return carriers.Select(MapCarrier).ToArray();
    }

    public async Task<ShippingCarrierResult?> GetActiveShippingCarrierByIdAsync(
        GetActiveShippingCarrierByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var carrier = await _shippingCarrierReadService.GetActiveByIdAsync(
            request.StoreId,
            request.CarrierId,
            cancellationToken);

        return carrier is null ? null : MapCarrier(carrier);
    }

    private static ShippingCarrierResult MapCarrier(ShippingCarrierDto carrier)
    {
        return new ShippingCarrierResult(
            carrier.Id,
            carrier.StoreId,
            carrier.Code,
            carrier.Name,
            carrier.ServiceCode,
            carrier.ServiceName,
            carrier.TrackingUrl,
            carrier.IsActive,
            carrier.SortOrder);
    }
}
