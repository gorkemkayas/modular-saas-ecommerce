using Shipment.Domain.Exceptions;

namespace Shipment.Domain.ValueObjects;

public sealed class ShipmentAddress
{
    private ShipmentAddress()
    {
    }

    private ShipmentAddress(
        string contactName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode)
    {
        ContactName = NormalizeRequired(contactName, "Contact name", 200);
        PhoneNumber = NormalizeRequired(phoneNumber, "Phone number", 50);
        Country = NormalizeRequired(country, "Country", 100);
        City = NormalizeRequired(city, "City", 100);
        District = NormalizeRequired(district, "District", 100);
        Line1 = NormalizeRequired(line1, "Address line 1", 250);
        Line2 = NormalizeOptional(line2, 250);
        PostalCode = NormalizeOptional(postalCode, 30);
    }

    public string ContactName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string District { get; private set; } = default!;
    public string Line1 { get; private set; } = default!;
    public string? Line2 { get; private set; }
    public string? PostalCode { get; private set; }

    public static ShipmentAddress Create(
        string contactName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode)
    {
        return new ShipmentAddress(
            contactName,
            phoneNumber,
            country,
            city,
            district,
            line1,
            line2,
            postalCode);
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
