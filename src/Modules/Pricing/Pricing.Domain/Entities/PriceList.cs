using Pricing.Domain.Common;
using Pricing.Domain.Enums;
using Pricing.Domain.Exceptions;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Entities;

public sealed class PriceList : IAggregateRoot
{
    private readonly List<PriceEntry> _entries = new();

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = default!;
    public Currency Currency { get; private set; } = default!;
    public int Priority { get; private set; }
    public bool IsDefault { get; private set; }
    public PriceListStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<PriceEntry> Entries => _entries.AsReadOnly();

    private PriceList()
    {
    }

    private PriceList(
        Guid id,
        Guid storeId,
        string name,
        Currency currency,
        int priority,
        bool isDefault)
    {
        if (storeId == Guid.Empty)
            throw new ArgumentException("StoreId cannot be empty.", nameof(storeId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Price list name cannot be empty.", nameof(name));

        Id = id;
        StoreId = storeId;
        Name = name.Trim();
        Currency = currency;
        Priority = priority;
        IsDefault = isDefault;
        Status = PriceListStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static PriceList Create(
        Guid storeId,
        string name,
        Currency currency,
        int priority = 0,
        bool isDefault = false)
    {
        return new PriceList(Guid.NewGuid(), storeId, name, currency, priority, isDefault);
    }

    public void Rename(string name)
    {
        EnsureMutable();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Price list name cannot be empty.", nameof(name));

        Name = name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangePriority(int priority)
    {
        EnsureMutable();
        Priority = priority;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDefault()
    {
        EnsureMutable();
        IsDefault = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UnmarkAsDefault()
    {
        EnsureMutable();
        IsDefault = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        EnsureNotArchived();
        Status = PriceListStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        EnsureNotArchived();
        Status = PriceListStatus.Inactive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = PriceListStatus.Archived;
        IsDefault = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetProductPrice(Guid productId, Money price, Money? compareAtPrice = null)
    {
        SetPriceInternal(PriceTarget.ForProduct(productId), price, compareAtPrice);
    }

    public void SetVariantPrice(Guid productId, Guid productVariantId, Money price, Money? compareAtPrice = null)
    {
        SetPriceInternal(PriceTarget.ForVariant(productId, productVariantId), price, compareAtPrice);
    }

    public void RemovePrice(Guid productId, Guid? productVariantId = null)
    {
        EnsureMutable();

        var entry = _entries.FirstOrDefault(x =>
            x.Target.ProductId == productId &&
            x.Target.ProductVariantId == productVariantId);

        if (entry is null)
            return;

        _entries.Remove(entry);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ActivatePriceEntry(Guid entryId)
    {
        EnsureMutable();

        var entry = _entries.FirstOrDefault(x => x.Id == entryId)
            ?? throw new PricingDomainException("Price entry was not found.");

        entry.Activate();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void DeactivatePriceEntry(Guid entryId)
    {
        EnsureMutable();

        var entry = _entries.FirstOrDefault(x => x.Id == entryId)
            ?? throw new PricingDomainException("Price entry was not found.");

        entry.Deactivate();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetPriceInternal(PriceTarget target, Money price, Money? compareAtPrice)
    {
        EnsureMutable();

        if (price.Currency != Currency)
            throw new PricingDomainException("Price currency must match the price list currency.");

        if (compareAtPrice is not null && compareAtPrice.Currency != Currency)
            throw new PricingDomainException("Compare-at price currency must match the price list currency.");

        var existingEntry = _entries.FirstOrDefault(x => x.Target == target);

        if (existingEntry is null)
        {
            _entries.Add(PriceEntry.Create(Id, target, price, compareAtPrice));
        }
        else
        {
            existingEntry.Update(price, compareAtPrice);
            existingEntry.Activate();
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void EnsureMutable()
    {
        EnsureNotArchived();
    }

    private void EnsureNotArchived()
    {
        if (Status == PriceListStatus.Archived)
            throw new PricingDomainException("Archived price list cannot be modified.");
    }
}
