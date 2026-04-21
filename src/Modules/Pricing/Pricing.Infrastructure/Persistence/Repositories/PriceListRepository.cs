using Microsoft.EntityFrameworkCore;
using Pricing.Domain.Entities;
using Pricing.Domain.Enums;
using Pricing.Domain.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Infrastructure.Persistence.Repositories;

public sealed class PriceListRepository : IPriceListRepository
{
    private readonly PricingDbContext _context;

    public PriceListRepository(PricingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default)
    {
        await _context.PriceLists.AddAsync(priceList, cancellationToken);
    }

    public Task<PriceList?> GetByIdAsync(Guid storeId, Guid priceListId, CancellationToken cancellationToken = default)
    {
        return _context.PriceLists
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == priceListId, cancellationToken);
    }

    public Task<PriceList?> GetDefaultByStoreAndCurrencyAsync(Guid storeId, Currency currency, CancellationToken cancellationToken = default)
    {
        return _context.PriceLists
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(
                x => x.StoreId == storeId
                    && x.Currency == currency
                    && x.IsDefault,
                cancellationToken);
    }

    public Task<bool> ExistsDefaultActiveListAsync(
        Guid storeId,
        Currency currency,
        Guid? excludedPriceListId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.PriceLists.AnyAsync(
            x => x.StoreId == storeId
                && x.Currency == currency
                && x.IsDefault
                && x.Status == PriceListStatus.Active
                && (!excludedPriceListId.HasValue || x.Id != excludedPriceListId.Value),
            cancellationToken);
    }
}
