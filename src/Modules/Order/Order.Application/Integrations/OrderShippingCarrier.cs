namespace Order.Application.Integrations;

public sealed record OrderShippingCarrier(
    Guid Id,
    string Code,
    string Name,
    string? ServiceCode,
    string? ServiceName,
    string? TrackingUrl);
