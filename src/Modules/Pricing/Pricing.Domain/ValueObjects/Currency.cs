using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class Currency : ValueObject
{
    public string Code { get; private set; } = default!;

    private Currency()
    {
    }

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty.", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || normalized.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Currency code must be a 3-letter ISO-like code.", nameof(code));

        return new Currency(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
