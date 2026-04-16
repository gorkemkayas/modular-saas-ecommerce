using Customer.Domain.Enums;
using Customer.Domain.Exceptions;
using Customer.Domain.ValueObjects;

namespace Customer.Domain.Entities;

public sealed class CustomerAddress
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public AddressType AddressType { get; private set; }
    public string Title { get; private set; } = default!;
    public string ContactName { get; private set; } = default!;
    public PhoneNumber PhoneNumber { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string District { get; private set; } = default!;
    public string Line1 { get; private set; } = default!;
    public string? Line2 { get; private set; }
    public string? PostalCode { get; private set; }
    public bool IsDefaultShipping { get; private set; }
    public bool IsDefaultBilling { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private CustomerAddress()
    {
    }

    private CustomerAddress(
        Guid id,
        Guid customerId,
        AddressType addressType,
        string title,
        string contactName,
        PhoneNumber phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode,
        bool isDefaultShipping,
        bool isDefaultBilling)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        Id = id;
        CustomerId = customerId;
        AddressType = addressType;
        Title = NormalizeRequired(title, nameof(title), 100);
        ContactName = NormalizeRequired(contactName, nameof(contactName), 200);
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Country = NormalizeRequired(country, nameof(country), 100);
        City = NormalizeRequired(city, nameof(city), 100);
        District = NormalizeRequired(district, nameof(district), 100);
        Line1 = NormalizeRequired(line1, nameof(line1), 500);
        Line2 = NormalizeOptional(line2, 500);
        PostalCode = NormalizeOptional(postalCode, 20);
        IsDefaultShipping = isDefaultShipping;
        IsDefaultBilling = isDefaultBilling;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static CustomerAddress Create(
        Guid customerId,
        AddressType addressType,
        string title,
        string contactName,
        PhoneNumber phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode,
        bool isDefaultShipping,
        bool isDefaultBilling)
    {
        return new CustomerAddress(
            Guid.NewGuid(),
            customerId,
            addressType,
            title,
            contactName,
            phoneNumber,
            country,
            city,
            district,
            line1,
            line2,
            postalCode,
            isDefaultShipping,
            isDefaultBilling);
    }

    public void Update(
        AddressType addressType,
        string title,
        string contactName,
        PhoneNumber phoneNumber,
        string country,
        string city,
        string district,
        string line1,
        string? line2,
        string? postalCode)
    {
        AddressType = addressType;
        Title = NormalizeRequired(title, nameof(title), 100);
        ContactName = NormalizeRequired(contactName, nameof(contactName), 200);
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Country = NormalizeRequired(country, nameof(country), 100);
        City = NormalizeRequired(city, nameof(city), 100);
        District = NormalizeRequired(district, nameof(district), 100);
        Line1 = NormalizeRequired(line1, nameof(line1), 500);
        Line2 = NormalizeOptional(line2, 500);
        PostalCode = NormalizeOptional(postalCode, 20);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDefaultShipping()
    {
        IsDefaultShipping = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UnmarkAsDefaultShipping()
    {
        if (!IsDefaultShipping)
            return;

        IsDefaultShipping = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDefaultBilling()
    {
        IsDefaultBilling = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UnmarkAsDefaultBilling()
    {
        if (!IsDefaultBilling)
            return;

        IsDefaultBilling = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeRequired(string value, string paramName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new CustomerDomainException($"{paramName} cannot be empty.");

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new CustomerDomainException($"{paramName} is too long.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new CustomerDomainException("Optional field is too long.");

        return normalized;
    }
}
