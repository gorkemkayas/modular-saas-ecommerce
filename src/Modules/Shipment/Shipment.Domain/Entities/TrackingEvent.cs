using Shipment.Domain.Enums;
using Shipment.Domain.Exceptions;

namespace Shipment.Domain.Entities;

public sealed class TrackingEvent
{
    private TrackingEvent()
    {
    }

    private TrackingEvent(
        Guid id,
        Guid shipmentPackageId,
        TrackingEventType type,
        DateTime occurredAtUtc,
        string? location,
        string description,
        string? rawStatusCode,
        string? rawStatusText)
    {
        if (shipmentPackageId == Guid.Empty)
            throw new ShipmentDomainException("Shipment package id is required.");

        if (occurredAtUtc == default)
            throw new ShipmentDomainException("Tracking event time is required.");

        Id = id;
        ShipmentPackageId = shipmentPackageId;
        Type = type;
        OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);
        Location = NormalizeOptional(location, 200);
        Description = NormalizeRequired(description, "Tracking event description", 500);
        RawStatusCode = NormalizeOptional(rawStatusCode, 100);
        RawStatusText = NormalizeOptional(rawStatusText, 500);
    }

    public Guid Id { get; private set; }
    public Guid ShipmentPackageId { get; private set; }
    public TrackingEventType Type { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string? Location { get; private set; }
    public string Description { get; private set; } = default!;
    public string? RawStatusCode { get; private set; }
    public string? RawStatusText { get; private set; }

    internal static TrackingEvent Create(
        Guid shipmentPackageId,
        TrackingEventType type,
        DateTime occurredAtUtc,
        string? location,
        string description,
        string? rawStatusCode,
        string? rawStatusText)
    {
        return new TrackingEvent(
            Guid.NewGuid(),
            shipmentPackageId,
            type,
            occurredAtUtc,
            location,
            description,
            rawStatusCode,
            rawStatusText);
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
