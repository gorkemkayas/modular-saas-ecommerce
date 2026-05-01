using Shipment.Domain.Enums;
using Shipment.Domain.Exceptions;

namespace Shipment.Domain.Entities;

public sealed class ShipmentPackage
{
    private readonly List<TrackingEvent> _trackingEvents = new();

    private ShipmentPackage()
    {
    }

    private ShipmentPackage(
        Guid id,
        Guid shipmentId,
        string packageNumber,
        string? trackingNumber,
        decimal? weight,
        string? weightUnit,
        string? labelReference)
    {
        if (shipmentId == Guid.Empty)
            throw new ShipmentDomainException("Shipment id is required.");

        if (weight.HasValue && weight.Value <= 0)
            throw new ShipmentDomainException("Package weight must be greater than zero.");

        if (weight.HasValue && string.IsNullOrWhiteSpace(weightUnit))
            throw new ShipmentDomainException("Weight unit is required when weight is provided.");

        Id = id;
        ShipmentId = shipmentId;
        PackageNumber = NormalizeRequired(packageNumber, "Package number", 50);
        TrackingNumber = NormalizeOptional(trackingNumber, 120);
        Weight = weight.HasValue ? decimal.Round(weight.Value, 3, MidpointRounding.AwayFromZero) : null;
        WeightUnit = NormalizeOptional(weightUnit, 20);
        LabelReference = NormalizeOptional(labelReference, 200);
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public string PackageNumber { get; private set; } = default!;
    public string? TrackingNumber { get; private set; }
    public decimal? Weight { get; private set; }
    public string? WeightUnit { get; private set; }
    public string? LabelReference { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ShippedAtUtc { get; private set; }

    public IReadOnlyCollection<TrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    internal static ShipmentPackage Create(
        Guid shipmentId,
        string packageNumber,
        string? trackingNumber,
        decimal? weight,
        string? weightUnit,
        string? labelReference)
    {
        var package = new ShipmentPackage(
            Guid.NewGuid(),
            shipmentId,
            packageNumber,
            trackingNumber,
            weight,
            weightUnit,
            labelReference);

        if (!string.IsNullOrWhiteSpace(labelReference))
        {
            package.AddTrackingEvent(
                TrackingEventType.LabelCreated,
                DateTime.UtcNow,
                null,
                "Carrier label created.",
                null,
                null);
        }

        return package;
    }

    internal void MarkShipped(DateTime shippedAtUtc)
    {
        if (ShippedAtUtc.HasValue)
            return;

        ShippedAtUtc = DateTime.SpecifyKind(shippedAtUtc, DateTimeKind.Utc);
    }

    internal void AddTrackingEvent(
        TrackingEventType type,
        DateTime occurredAtUtc,
        string? location,
        string description,
        string? rawStatusCode,
        string? rawStatusText)
    {
        if (_trackingEvents.Count > 0 && occurredAtUtc < _trackingEvents.Max(x => x.OccurredAtUtc))
            throw new ShipmentDomainException("Tracking event time cannot go backwards within the same package.");

        _trackingEvents.Add(TrackingEvent.Create(
            Id,
            type,
            occurredAtUtc,
            location,
            description,
            rawStatusCode,
            rawStatusText));
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
