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

        var defaultPriceList = await _context.PriceLists
            .AsNoTracking()
            .Include(x => x.Entries)
            .Where(x =>
                x.StoreId == storeId &&
                x.IsDefault &&
                x.Status == PriceListStatus.Active &&
                x.Currency == currency)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultPriceList is null)
            return null;

        var resolvedEntry = defaultPriceList.Entries.FirstOrDefault(x =>
            x.Target.ProductId == productId &&
            x.Target.ProductVariantId == productVariantId &&
            x.IsActive);

        if (resolvedEntry is null)
            return null;

        return new ResolvedPriceDto(
            defaultPriceList.StoreId,
            resolvedEntry.Target.ProductId,
            resolvedEntry.Target.ProductVariantId,
            resolvedEntry.Price.Amount,
            resolvedEntry.Price.Currency.Code,
            resolvedEntry.CompareAtPrice?.Amount,
            defaultPriceList.Id,
            resolvedEntry.Id);
    }
}
