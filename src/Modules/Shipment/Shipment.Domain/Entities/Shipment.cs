using Shipment.Domain.Common;
using Shipment.Domain.Enums;
using Shipment.Domain.Exceptions;
using Shipment.Domain.ValueObjects;

namespace Shipment.Domain.Entities;

public sealed class Shipment : IAggregateRoot
{
    private readonly List<ShipmentLine> _lines = new();
    private readonly List<ShipmentPackage> _packages = new();

    private Shipment()
    {
    }

    private Shipment(
        Guid id,
        Guid storeId,
        Guid orderId,
        string orderNumber,
        string shipmentNumber,
        string recipientName,
        string recipientPhoneNumber,
        ShipmentAddress destinationAddress,
        IReadOnlyCollection<ShipmentLineDraft> lines,
        string? internalNote)
    {
        if (storeId == Guid.Empty)
            throw new ShipmentDomainException("Store id is required.");

        if (orderId == Guid.Empty)
            throw new ShipmentDomainException("Order id is required.");

        if (lines.Count == 0)
            throw new ShipmentDomainException("Shipment must contain at least one line.");

        Id = id;
        StoreId = storeId;
        OrderId = orderId;
        OrderNumber = NormalizeRequired(orderNumber, "Order number", 50);
        ShipmentNumber = NormalizeRequired(shipmentNumber, "Shipment number", 50);
        RecipientName = NormalizeRequired(recipientName, "Recipient name", 200);
        RecipientPhoneNumber = NormalizeRequired(recipientPhoneNumber, "Recipient phone number", 50);
        DestinationAddress = destinationAddress ?? throw new ArgumentNullException(nameof(destinationAddress));
        InternalNote = NormalizeOptional(internalNote, 1000);
        Status = ShipmentStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;

        foreach (var line in lines)
        {
            _lines.Add(ShipmentLine.Create(
                Id,
                line.OrderItemId,
                line.ProductId,
                line.ProductVariantId,
                line.ProductName,
                line.VariantName,
                line.Sku,
                line.Quantity));
        }
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid OrderId { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public string ShipmentNumber { get; private set; } = default!;
    public ShipmentStatus Status { get; private set; }
    public string RecipientName { get; private set; } = default!;
    public string RecipientPhoneNumber { get; private set; } = default!;
    public ShipmentAddress DestinationAddress { get; private set; } = default!;
    public string? CarrierCode { get; private set; }
    public string? CarrierName { get; private set; }
    public string? ServiceCode { get; private set; }
    public string? ServiceName { get; private set; }
    public string? TrackingUrl { get; private set; }
    public string? InternalNote { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? ReadyForDispatchAtUtc { get; private set; }
    public DateTime? ShippedAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<ShipmentLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<ShipmentPackage> Packages => _packages.AsReadOnly();

    public static Shipment Create(
        Guid storeId,
        Guid orderId,
        string orderNumber,
        string shipmentNumber,
        string recipientName,
        string recipientPhoneNumber,
        ShipmentAddress destinationAddress,
        IReadOnlyCollection<ShipmentLineDraft> lines,
        string? internalNote = null)
    {
        return new Shipment(
            Guid.NewGuid(),
            storeId,
            orderId,
            orderNumber,
            shipmentNumber,
            recipientName,
            recipientPhoneNumber,
            destinationAddress,
            lines,
            internalNote);
    }

    public void AssignCarrier(
        string carrierCode,
        string carrierName,
        string? serviceCode,
        string? serviceName,
        string? trackingUrl)
    {
        EnsureNotTerminal();

        CarrierCode = NormalizeRequired(carrierCode, "Carrier code", 50);
        CarrierName = NormalizeRequired(carrierName, "Carrier name", 200);
        ServiceCode = NormalizeOptional(serviceCode, 50);
        ServiceName = NormalizeOptional(serviceName, 200);
        TrackingUrl = NormalizeOptional(trackingUrl, 500);
        Touch();
    }

    public Guid AddPackage(
        string packageNumber,
        string? trackingNumber,
        decimal? weight,
        string? weightUnit,
        string? labelReference)
    {
        EnsureNotTerminal();

        var normalizedPackageNumber = NormalizeRequired(packageNumber, "Package number", 50);

        if (_packages.Any(x => x.PackageNumber == normalizedPackageNumber))
            throw new ShipmentDomainException("Package number already exists in this shipment.");

        var package = ShipmentPackage.Create(
            Id,
            normalizedPackageNumber,
            trackingNumber,
            weight,
            weightUnit,
            labelReference);

        _packages.Add(package);
        Touch();
        return package.Id;
    }

    public void MarkReadyForDispatch()
    {
        EnsureNotTerminal();

        if (Status is ShipmentStatus.Shipped or ShipmentStatus.Delivered or ShipmentStatus.DeliveryException)
            throw new ShipmentDomainException("Shipped shipment cannot move back to ready for dispatch.");

        if (_packages.Count == 0)
            throw new ShipmentDomainException("Shipment must contain at least one package before dispatch.");

        Status = ShipmentStatus.ReadyForDispatch;
        ReadyForDispatchAtUtc ??= DateTime.UtcNow;
        Touch();
    }

    public void MarkShipped(DateTime? shippedAtUtc = null)
    {
        EnsureNotCancelled();

        if (_packages.Count == 0)
            throw new ShipmentDomainException("Shipment must contain at least one package before shipping.");

        if (Status == ShipmentStatus.Delivered)
            throw new ShipmentDomainException("Delivered shipment cannot be shipped again.");

        var effectiveShippedAt = DateTime.SpecifyKind(shippedAtUtc ?? DateTime.UtcNow, DateTimeKind.Utc);

        foreach (var package in _packages)
        {
            package.MarkShipped(effectiveShippedAt);

            if (!package.TrackingEvents.Any(x => x.Type == TrackingEventType.PickedUp))
            {
                package.AddTrackingEvent(
                    TrackingEventType.PickedUp,
                    effectiveShippedAt,
                    null,
                    "Shipment handed over to carrier.",
                    null,
                    null);
            }
        }

        Status = ShipmentStatus.Shipped;
        ReadyForDispatchAtUtc ??= effectiveShippedAt;
        ShippedAtUtc = effectiveShippedAt;
        Touch();
    }

    public void RegisterTrackingEvent(
        Guid packageId,
        TrackingEventType type,
        DateTime occurredAtUtc,
        string? location,
        string description,
        string? rawStatusCode,
        string? rawStatusText)
    {
        EnsureNotCancelled();

        var package = FindPackage(packageId);
        package.AddTrackingEvent(
            type,
            DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc),
            location,
            description,
            rawStatusCode,
            rawStatusText);

        Touch();
    }

    public void MarkDelivered(DateTime? deliveredAtUtc = null)
    {
        EnsureNotCancelled();

        if (Status is not ShipmentStatus.Shipped and not ShipmentStatus.DeliveryException)
            throw new ShipmentDomainException("Only shipped shipment can be delivered.");

        var effectiveDeliveredAt = DateTime.SpecifyKind(deliveredAtUtc ?? DateTime.UtcNow, DateTimeKind.Utc);

        foreach (var package in _packages)
        {
            if (!package.TrackingEvents.Any(x => x.Type == TrackingEventType.Delivered))
            {
                package.AddTrackingEvent(
                    TrackingEventType.Delivered,
                    effectiveDeliveredAt,
                    null,
                    "Shipment delivered to recipient.",
                    null,
                    null);
            }
        }

        Status = ShipmentStatus.Delivered;
        DeliveredAtUtc = effectiveDeliveredAt;
        Touch();
    }

    public void MarkDeliveryException(
        Guid packageId,
        DateTime occurredAtUtc,
        string description,
        string? location,
        string? rawStatusCode,
        string? rawStatusText)
    {
        EnsureNotCancelled();

        if (Status is not ShipmentStatus.Shipped and not ShipmentStatus.DeliveryException)
            throw new ShipmentDomainException("Only shipped shipment can move to delivery exception.");

        if (Status == ShipmentStatus.Delivered)
            throw new ShipmentDomainException("Delivered shipment cannot move to delivery exception.");

        var package = FindPackage(packageId);
        package.AddTrackingEvent(
            TrackingEventType.Exception,
            DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc),
            location,
            description,
            rawStatusCode,
            rawStatusText);

        Status = ShipmentStatus.DeliveryException;
        Touch();
    }

    public void Cancel(string? reason)
    {
        if (Status == ShipmentStatus.Cancelled)
            return;

        if (Status is ShipmentStatus.Shipped or ShipmentStatus.Delivered or ShipmentStatus.DeliveryException)
            throw new ShipmentDomainException("Shipped or terminal shipment cannot be cancelled.");

        Status = ShipmentStatus.Cancelled;
        CancellationReason = NormalizeOptional(reason, 500);
        CancelledAtUtc = DateTime.UtcNow;
        Touch();
    }

    private ShipmentPackage FindPackage(Guid packageId)
    {
        if (packageId == Guid.Empty)
            throw new ShipmentDomainException("Shipment package id is required.");

        return _packages.FirstOrDefault(x => x.Id == packageId)
            ?? throw new ShipmentDomainException("Shipment package was not found.");
    }

    private void EnsureNotTerminal()
    {
        if (Status is ShipmentStatus.Delivered or ShipmentStatus.Cancelled)
            throw new ShipmentDomainException("Terminal shipment cannot be modified.");
    }

    private void EnsureNotCancelled()
    {
        if (Status == ShipmentStatus.Cancelled)
            throw new ShipmentDomainException("Cancelled shipment cannot be modified.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ShipmentDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new ShipmentDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new ShipmentDomainException($"Value cannot exceed {maxLength} characters.");

        return normalized;
    }
}

public sealed record ShipmentLineDraft(
    Guid OrderItemId,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string? Sku,
    int Quantity);
