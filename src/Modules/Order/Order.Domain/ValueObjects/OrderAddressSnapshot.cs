using Order.Domain.Common;
using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public sealed class OrderAddressSnapshot : ValueObject
{
    private OrderAddressSnapshot()
    {
    }

    private OrderAddressSnapshot(
        string title,
        string contactName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode)
    {
        Title = title;
        ContactName = contactName;
        PhoneNumber = phoneNumber;
        Country = country;
        City = city;
        District = district;
        Line1 = line1;
        Line2 = line2;
        PostalCode = postalCode;
    }

    public string Title { get; private set; } = default!;
    public string ContactName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string District { get; private set; } = default!;
    public string Line1 { get; private set; } = default!;
    public string? Line2 { get; private set; }
    public string? PostalCode { get; private set; }

    public static OrderAddressSnapshot Create(
        string title,
        string contactName,
        string phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode)
    {
        return new OrderAddressSnapshot(
            NormalizeRequired(title, "Address title"),
            NormalizeRequired(contactName, "Address contact name"),
            NormalizeRequired(phoneNumber, "Address phone number"),
            NormalizeRequired(country, "Address country"),
            NormalizeRequired(city, "Address city"),
            NormalizeRequired(district, "Address district"),
            NormalizeRequired(line1, "Address line1"),
            NormalizeOptional(line2, 250),
            NormalizeOptional(postalCode, 30));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return ContactName;
        yield return PhoneNumber;
        yield return Country;
        yield return City;
        yield return District;
        yield return Line1;
        yield return Line2;
        yield return PostalCode;
    }

    private static string NormalizeRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OrderDomainException($"{fieldName} is required.");

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new OrderDomainException($"{nameof(OrderAddressSnapshot)} field is too long.");

        return normalized;
    }
}
