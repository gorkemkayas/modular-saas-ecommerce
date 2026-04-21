using Pricing.Domain.Common;
using Pricing.Domain.Exceptions;

namespace Pricing.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; } = default!;

    private Money()
    {
    }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0m)
            throw new PricingDomainException("Money amount cannot be negative.");

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency);
    }

    public static Money Create(decimal amount, string currencyCode)
    {
        return Create(amount, Currency.Create(currencyCode));
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        if (Amount - other.Amount < 0m)
            throw new PricingDomainException("Money subtraction result cannot be negative.");

        return Create(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new PricingDomainException("Money currency mismatch.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency.Code}";
}
