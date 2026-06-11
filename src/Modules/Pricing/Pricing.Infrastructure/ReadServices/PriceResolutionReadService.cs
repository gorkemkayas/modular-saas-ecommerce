using Microsoft.EntityFrameworkCore;
using Pricing.Application.Abstractions.Queries;
using Pricing.Application.Prices.DTOs;
using Pricing.Domain.Enums;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.ReadServices;

public sealed class PriceResolutionReadService : IPriceResolutionReadService
{
    private readonly PricingDbContext _context;

    public PriceResolutionReadService(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<ResolvedPriceDto?> GetResolvedPriceAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        var currency = Currency.Create(currencyCode);
        var defaultPriceLists = await _context.PriceLists
            .AsNoTracking()
            .Include(x => x.Entries)
            .Where(x =>
                x.StoreId == storeId &&
                x.IsDefault &&
                x.Status == PriceListStatus.Active)
            .OrderByDescending(x => x.Currency == currency)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        foreach (var priceList in defaultPriceLists)
        {
            var resolvedEntry = priceList.Entries.FirstOrDefault(x =>
                x.Target.ProductId == productId &&
                x.Target.ProductVariantId == productVariantId &&
                x.IsActive);

            if (resolvedEntry is null)
                continue;

            return new ResolvedPriceDto(
                priceList.StoreId,
                resolvedEntry.Target.ProductId,
                resolvedEntry.Target.ProductVariantId,
                resolvedEntry.Price.Amount,
                resolvedEntry.Price.Currency.Code,
                resolvedEntry.CompareAtPrice?.Amount,
                priceList.Id,
                resolvedEntry.Id);
        }

        return null;
    }
}
