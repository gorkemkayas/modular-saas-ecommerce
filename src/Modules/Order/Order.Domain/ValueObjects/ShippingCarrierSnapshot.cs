using Order.Domain.Common;
using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public sealed class ShippingCarrierSnapshot : ValueObject
{
    private ShippingCarrierSnapshot()
    {
    }

    private ShippingCarrierSnapshot(
        Guid carrierId,
        string code,
        string name,
        string? serviceCode,
        string? serviceName,
        string? trackingUrl)
    {
        CarrierId = carrierId;
        Code = code;
        Name = name;
        ServiceCode = serviceCode;
        ServiceName = serviceName;
        TrackingUrl = trackingUrl;
    }

    public Guid CarrierId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? ServiceCode { get; private set; }
    public string? ServiceName { get; private set; }
    public string? TrackingUrl { get; private set; }

    public static ShippingCarrierSnapshot Create(
        Guid carrierId,
        string code,
        string name,
        string? serviceCode,
        string? serviceName,
        string? trackingUrl)
    {
        if (carrierId == Guid.Empty)
            throw new OrderDomainException("Shipping carrier id is required.");

        return new ShippingCarrierSnapshot(
            carrierId,
            NormalizeRequired(code, "Shipping carrier code", 50),
            NormalizeRequired(name, "Shipping carrier name", 200),
            NormalizeOptional(serviceCode, 50),
            NormalizeOptional(serviceName, 200),
            NormalizeOptional(trackingUrl, 500));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CarrierId;
        yield return Code;
        yield return Name;
        yield return ServiceCode;
        yield return ServiceName;
        yield return TrackingUrl;
    }

    private static string NormalizeRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OrderDomainException($"{fieldName} is required.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new OrderDomainException($"{fieldName} cannot exceed {maxLength} characters.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new OrderDomainException("Shipping carrier value is too long.");

        return normalized;
    }
}
