using Order.Domain.Common;
using Order.Domain.Exceptions;

namespace Order.Domain.ValueObjects;

public sealed class OrderPriceSnapshot : ValueObject
{
    private OrderPriceSnapshot()
    {
    }

    private OrderPriceSnapshot(
        decimal amount,
        string currencyCode,
        decimal? compareAtAmount,
        Guid priceListId,
        Guid priceEntryId)
    {
        Amount = amount;
        CurrencyCode = currencyCode;
        CompareAtAmount = compareAtAmount;
        PriceListId = priceListId;
        PriceEntryId = priceEntryId;
    }

    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = default!;
    public decimal? CompareAtAmount { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid PriceEntryId { get; private set; }

    public static OrderPriceSnapshot Create(
        decimal amount,
        string currencyCode,
        decimal? compareAtAmount,
        Guid priceListId,
        Guid priceEntryId)
    {
        if (amount < 0)
            throw new OrderDomainException("Price amount cannot be negative.");

        if (compareAtAmount.HasValue && compareAtAmount.Value < amount)
            throw new OrderDomainException("Compare-at amount cannot be lower than amount.");

        if (priceListId == Guid.Empty)
            throw new OrderDomainException("Price list id is required.");

        if (priceEntryId == Guid.Empty)
            throw new OrderDomainException("Price entry id is required.");

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new OrderDomainException("Currency code is required.");

        return new OrderPriceSnapshot(
            amount,
            currencyCode.Trim().ToUpperInvariant(),
            compareAtAmount,
            priceListId,
            priceEntryId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return CurrencyCode;
        yield return CompareAtAmount;
        yield return PriceListId;
        yield return PriceEntryId;
    }
}
