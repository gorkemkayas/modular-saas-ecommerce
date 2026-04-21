using Pricing.Application.Common.Models;
using Pricing.Application.PriceLists.DTOs;
using Pricing.Domain.Enums;

namespace Pricing.Application.Abstractions.Queries;

public interface IPriceListReadService
{
    Task<PriceListDto?> GetByIdAsync(Guid storeId, Guid priceListId, CancellationToken cancellationToken = default);
    Task<PagedResult<PriceListSummaryDto>> SearchAsync(
        PriceListSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
