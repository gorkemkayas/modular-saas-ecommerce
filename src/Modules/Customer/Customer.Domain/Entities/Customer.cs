using Customer.Domain.Common;
using Customer.Domain.Enums;
using Customer.Domain.Exceptions;
using Customer.Domain.ValueObjects;

namespace Customer.Domain.Entities;

public sealed class Customer : IAggregateRoot
{
    private readonly List<CustomerAddress> _addresses = new();
    private readonly List<CustomerConsent> _consents = new();

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ExternalUserId { get; private set; }
    public EmailAddress Email { get; private set; } = default!;
    public PersonName Name { get; private set; } = default!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public CustomerStatus Status { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public string? PreferredCurrency { get; private set; }
    public DateTime RegisteredAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();
    public IReadOnlyCollection<CustomerConsent> Consents => _consents.AsReadOnly();

    private Customer()
    {
    }

    private Customer(
        Guid id,
        Guid tenantId,
        Guid externalUserId,
        EmailAddress email,
        PersonName name,
        PhoneNumber? phoneNumber,
        string? preferredLanguage,
        string? preferredCurrency)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));

        if (externalUserId == Guid.Empty)
            throw new ArgumentException("ExternalUserId cannot be empty.", nameof(externalUserId));

        Id = id;
        TenantId = tenantId;
        ExternalUserId = externalUserId;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PhoneNumber = phoneNumber;
        PreferredLanguage = NormalizeOptional(preferredLanguage, 10);
        PreferredCurrency = NormalizeOptional(preferredCurrency, 3)?.ToUpperInvariant();
        Status = CustomerStatus.Active;
        RegisteredAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Customer Create(
        Guid tenantId,
        Guid externalUserId,
        EmailAddress email,
        PersonName name,
        PhoneNumber? phoneNumber = null,
        string? preferredLanguage = null,
        string? preferredCurrency = null)
    {
        return new Customer(
            Guid.NewGuid(),
            tenantId,
            externalUserId,
            email,
            name,
            phoneNumber,
            preferredLanguage,
            preferredCurrency);
    }

    public void SyncIdentity(EmailAddress email, PersonName name)
    {
        EnsureNotArchived();

        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateProfile(PersonName name, PhoneNumber? phoneNumber)
    {
        EnsureNotArchived();

        Name = name ?? throw new ArgumentNullException(nameof(name));
        PhoneNumber = phoneNumber;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePreferences(string? preferredLanguage, string? preferredCurrency)
    {
        EnsureNotArchived();

        PreferredLanguage = NormalizeOptional(preferredLanguage, 10);
        PreferredCurrency = NormalizeOptional(preferredCurrency, 3)?.ToUpperInvariant();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Guid AddAddress(
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
        EnsureNotArchived();

        if (_addresses.Count >= 20)
            throw new CustomerDomainException("Customer cannot have more than 20 saved addresses.");

        var address = CustomerAddress.Create(
            Id,
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
            isDefaultShipping || !_addresses.Any(x => x.IsDefaultShipping),
            isDefaultBilling || !_addresses.Any(x => x.IsDefaultBilling));

        if (address.IsDefaultShipping)
        {
            foreach (var item in _addresses)
                item.UnmarkAsDefaultShipping();
        }

        if (address.IsDefaultBilling)
        {
            foreach (var item in _addresses)
                item.UnmarkAsDefaultBilling();
        }

        _addresses.Add(address);
        UpdatedAtUtc = DateTime.UtcNow;

        return address.Id;
    }

    public void UpdateAddress(
        Guid addressId,
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
        EnsureNotArchived();

        var address = GetAddressOrThrow(addressId);
        address.Update(
            addressType,
            title,
            contactName,
            phoneNumber,
            country,
            city,
            district,
            line1,
            line2,
            postalCode);

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveAddress(Guid addressId)
    {
        EnsureNotArchived();

        var address = GetAddressOrThrow(addressId);
        var wasDefaultShipping = address.IsDefaultShipping;
        var wasDefaultBilling = address.IsDefaultBilling;

        _addresses.Remove(address);

        if (wasDefaultShipping && _addresses.Count > 0)
            _addresses[0].MarkAsDefaultShipping();

        if (wasDefaultBilling && _addresses.Count > 0)
        {
            var billingTarget = _addresses.FirstOrDefault(x => x.Id != addressId) ?? _addresses[0];
            billingTarget.MarkAsDefaultBilling();
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetDefaultShippingAddress(Guid addressId)
    {
        EnsureNotArchived();

        var address = GetAddressOrThrow(addressId);

        foreach (var item in _addresses)
            item.UnmarkAsDefaultShipping();

        address.MarkAsDefaultShipping();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetDefaultBillingAddress(Guid addressId)
    {
        EnsureNotArchived();

        var address = GetAddressOrThrow(addressId);

        foreach (var item in _addresses)
            item.UnmarkAsDefaultBilling();

        address.MarkAsDefaultBilling();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpsertConsent(ConsentType consentType, bool isGranted, string source)
    {
        EnsureNotArchived();

        var existingConsent = _consents.FirstOrDefault(x => x.ConsentType == consentType);

        if (existingConsent is null)
        {
            _consents.Add(CustomerConsent.Create(Id, consentType, isGranted, source));
        }
        else
        {
            existingConsent.Update(isGranted, source);
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Block()
    {
        EnsureNotArchived();

        Status = CustomerStatus.Blocked;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == CustomerStatus.Archived)
            throw new CustomerDomainException("Archived customer cannot be activated.");

        Status = CustomerStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = CustomerStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private CustomerAddress GetAddressOrThrow(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(x => x.Id == addressId);

        if (address is null)
            throw new CustomerDomainException($"Customer address {addressId} was not found.");

        return address;
    }

    private void EnsureNotArchived()
    {
        if (Status == CustomerStatus.Archived)
            throw new CustomerDomainException("Archived customer cannot be modified.");
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new CustomerDomainException("Field length exceeds the allowed limit.");

        return normalized;
    }
}
