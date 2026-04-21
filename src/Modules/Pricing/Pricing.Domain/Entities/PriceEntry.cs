using Pricing.Domain.Exceptions;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public sealed class PriceEntry
{
    public Guid Id { get; private set; }
    public Guid PriceListId { get; private set; }
    public PriceTarget Target { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public Money? CompareAtPrice { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private PriceEntry()
    {
    }

    private PriceEntry(
        Guid id,
        Guid priceListId,
        PriceTarget target,
        Money price,
        Money? compareAtPrice)
    {
        if (priceListId == Guid.Empty)
            throw new ArgumentException("PriceListId cannot be empty.", nameof(priceListId));

        EnsureCompareAtPrice(price, compareAtPrice);

        Id = id;
        PriceListId = priceListId;
        Target = target;
        Price = price;
        CompareAtPrice = compareAtPrice;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static PriceEntry Create(
        Guid priceListId,
        PriceTarget target,
        Money price,
        Money? compareAtPrice = null)
    {
        return new PriceEntry(Guid.NewGuid(), priceListId, target, price, compareAtPrice);
    }

    public void Update(Money price, Money? compareAtPrice = null)
    {
        EnsureCompareAtPrice(price, compareAtPrice);

        Price = price;
        CompareAtPrice = compareAtPrice;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void EnsureCompareAtPrice(Money price, Money? compareAtPrice)
    {
        if (compareAtPrice is null)
            return;

        if (compareAtPrice.Currency != price.Currency)
            throw new PricingDomainException("Compare-at price currency must match price currency.");

        if (compareAtPrice.Amount < price.Amount)
            throw new PricingDomainException("Compare-at price cannot be lower than actual price.");
    }
}
