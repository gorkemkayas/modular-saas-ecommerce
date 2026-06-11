using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Repositories;

public interface IPriceListRepository
{
    Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdAsync(Guid storeId, Guid priceListId, CancellationToken cancellationToken = default);
    Task<PriceList?> GetDefaultByStoreAndCurrencyAsync(Guid storeId, Currency currency, CancellationToken cancellationToken = default);
    Task<int> CountNonArchivedByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsDefaultActiveListAsync(
        Guid storeId,
        Currency currency,
        Guid? excludedPriceListId = null,
        CancellationToken cancellationToken = default);
}
