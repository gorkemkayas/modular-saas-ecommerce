namespace Order.Application.Integrations;

public sealed record OrderCustomerContext(
    Guid CustomerId,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? PreferredCurrency,
    OrderAddressSnapshotData ShippingAddress,
    OrderAddressSnapshotData BillingAddress);
