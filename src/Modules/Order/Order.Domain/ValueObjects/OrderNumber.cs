using Order.Domain.Common;
using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public sealed class OrderNumber : ValueObject
{
    private OrderNumber()
    {
    }

    private OrderNumber(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = default!;

    public static OrderNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new OrderDomainException("Order number cannot be empty.");

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > 40)
            throw new OrderDomainException("Order number cannot exceed 40 characters.");

        return new OrderNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
