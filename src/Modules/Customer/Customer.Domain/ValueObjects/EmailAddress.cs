using Customer.Domain.Common;
using System.Text.RegularExpressions;

namespace Customer.Domain.ValueObjects;

public sealed class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 320)
            throw new ArgumentException("Email address is too long.", nameof(value));

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException("Email address format is invalid.", nameof(value));

        return new EmailAddress(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
