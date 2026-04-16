using Customer.Domain.Enums;
using Customer.Domain.Exceptions;

namespace Customer.Domain.Entities;

public sealed class CustomerConsent
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public ConsentType ConsentType { get; private set; }
    public bool IsGranted { get; private set; }
    public string Source { get; private set; } = default!;
    public DateTime UpdatedAtUtc { get; private set; }

    private CustomerConsent()
    {
    }

    private CustomerConsent(
        Guid id,
        Guid customerId,
        ConsentType consentType,
        bool isGranted,
        string source)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        Id = id;
        CustomerId = customerId;
        ConsentType = consentType;
        IsGranted = isGranted;
        Source = NormalizeSource(source);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static CustomerConsent Create(
        Guid customerId,
        ConsentType consentType,
        bool isGranted,
        string source)
    {
        return new CustomerConsent(Guid.NewGuid(), customerId, consentType, isGranted, source);
    }

    public void Update(bool isGranted, string source)
    {
        IsGranted = isGranted;
        Source = NormalizeSource(source);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new CustomerDomainException("Consent source cannot be empty.");

        var normalized = source.Trim();

        if (normalized.Length > 100)
            throw new CustomerDomainException("Consent source is too long.");

        return normalized;
    }
}
