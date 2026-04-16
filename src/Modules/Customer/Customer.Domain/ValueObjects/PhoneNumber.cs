using Customer.Domain.Common;

namespace Customer.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));

        var normalized = Normalize(value);

        if (normalized.Length is < 7 or > 20)
            throw new ArgumentException("Phone number length is invalid.", nameof(value));

        return new PhoneNumber(normalized);
    }

    public static PhoneNumber? CreateNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : Create(value);
    }

    private static string Normalize(string value)
    {
        var chars = value
            .Trim()
            .Where(ch => char.IsDigit(ch) || ch == '+')
            .ToArray();

        var normalized = new string(chars);

        if (normalized.Count(ch => ch == '+') > 1 || (normalized.Contains('+') && normalized[0] != '+'))
            throw new ArgumentException("Phone number format is invalid.", nameof(value));

        return normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
