namespace Order.Application.Integrations;

public sealed record OrderAddressSnapshotData(
    string Title,
    string ContactName,
    string PhoneNumber,
    string Country,
    string City,
    string District,
    string Line1,
    string? Line2,
    string? PostalCode);
