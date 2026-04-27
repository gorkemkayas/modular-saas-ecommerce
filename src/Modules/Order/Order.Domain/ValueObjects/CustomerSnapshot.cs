using Order.Domain.Common;
using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public sealed class CustomerSnapshot : ValueObject
{
    private CustomerSnapshot()
    {
    }

    private CustomerSnapshot(Guid customerId, string email, string fullName, string? phoneNumber)
    {
        CustomerId = customerId;
        Email = email;
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public Guid CustomerId { get; private set; }
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }

    public static CustomerSnapshot Create(Guid customerId, string email, string fullName, string? phoneNumber)
    {
        if (customerId == Guid.Empty)
            throw new OrderDomainException("Customer id is required.");

        var normalizedEmail = NormalizeRequired(email, "Customer email");
        var normalizedFullName = NormalizeRequired(fullName, "Customer full name");
        var normalizedPhoneNumber = NormalizeOptional(phoneNumber, 50);

        return new CustomerSnapshot(customerId, normalizedEmail, normalizedFullName, normalizedPhoneNumber);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CustomerId;
        yield return Email;
        yield return FullName;
        yield return PhoneNumber;
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
            throw new OrderDomainException("Customer phone number is too long.");

        return normalized;
    }
}
