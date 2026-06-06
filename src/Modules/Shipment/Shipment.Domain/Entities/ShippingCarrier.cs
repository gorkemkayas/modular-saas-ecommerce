using Shipment.Domain.Common;
using Shipment.Domain.Exceptions;

namespace Shipment.Domain.Entities;

public sealed class ShippingCarrier : IAggregateRoot
{
    private ShippingCarrier()
    {
    }

    private ShippingCarrier(
        Guid id,
        Guid storeId,
        string code,
        string name,
        string? serviceCode,
        string? serviceName,
        string? trackingUrl,
        int sortOrder)
    {
        if (storeId == Guid.Empty)
            throw new ShipmentDomainException("Store id is required.");

        Id = id;
        StoreId = storeId;
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, "Carrier name", 200);
        ServiceCode = NormalizeOptional(serviceCode, 50);
        ServiceName = NormalizeOptional(serviceName, 200);
        TrackingUrl = NormalizeOptional(trackingUrl, 500);
        SortOrder = sortOrder;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? ServiceCode { get; private set; }
    public string? ServiceName { get; private set; }
    public string? TrackingUrl { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static ShippingCarrier Create(
        Guid storeId,
        string code,
        string name,
        string? serviceCode,
        string? serviceName,
        string? trackingUrl,
        int sortOrder)
    {
        return new ShippingCarrier(
            Guid.NewGuid(),
            storeId,
            code,
            name,
            serviceCode,
            serviceName,
            trackingUrl,
            sortOrder);
    }

    public void Update(
        string code,
        string name,
        string? serviceCode,
        string? serviceName,
        string? trackingUrl,
        bool isActive,
        int sortOrder)
    {
        Code = NormalizeCode(code);
        Name = NormalizeRequired(name, "Carrier name", 200);
        ServiceCode = NormalizeOptional(serviceCode, 50);
        ServiceName = NormalizeOptional(serviceName, 200);
        TrackingUrl = NormalizeOptional(trackingUrl, 500);
        IsActive = isActive;
        SortOrder = sortOrder;
        Touch();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static string NormalizeCode(string value)
    {
        var normalized = NormalizeRequired(value, "Carrier code", 50).ToLowerInvariant();

        if (normalized.Any(char.IsWhiteSpace))
            throw new ShipmentDomainException("Carrier code cannot contain whitespace.");

        return normalized;
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
