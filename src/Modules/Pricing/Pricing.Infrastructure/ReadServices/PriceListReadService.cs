using Microsoft.EntityFrameworkCore;
using Pricing.Application.Abstractions.Queries;
using Pricing.Application.Common.Models;
using Pricing.Application.PriceLists.DTOs;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.ReadServices;

public sealed class PriceListReadService : IPriceListReadService
{
    private readonly PricingDbContext _context;

    public PriceListReadService(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<PriceListDto?> GetByIdAsync(Guid storeId, Guid priceListId, CancellationToken cancellationToken = default)
    {
        var priceList = await _context.PriceLists
            .AsNoTracking()
            .Include(x => x.Entries)
            .FirstOrDefaultAsync(x => x.StoreId == storeId && x.Id == priceListId, cancellationToken);

        if (priceList is null)
            return null;

        return new PriceListDto(
            priceList.Id,
            priceList.StoreId,
            priceList.Name,
            priceList.Currency.Code,
            priceList.Priority,
            priceList.IsDefault,
            priceList.Status,
            priceList.CreatedAtUtc,
            priceList.UpdatedAtUtc,
            priceList.Entries
                .OrderBy(x => x.Target.ProductVariantId.HasValue)
                .ThenBy(x => x.Target.ProductId)
                .ThenBy(x => x.Target.ProductVariantId)
                .Select(x => new PriceEntryDto(
                    x.Id,
                    x.Target.ProductId,
                    x.Target.ProductVariantId,
                    x.Price.Amount,
                    x.Price.Currency.Code,
                    x.CompareAtPrice?.Amount,
                    x.IsActive))
                .ToArray());
    }

    public async Task<PagedResult<PriceListSummaryDto>> SearchAsync(
        PriceListSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PriceLists
            .AsNoTracking()
            .Where(x => x.StoreId == criteria.StoreId);

        if (!string.IsNullOrWhiteSpace(criteria.CurrencyCode))
        {
            var currency = Currency.Create(criteria.CurrencyCode);
            query = query.Where(x => x.Currency == currency);
        }

        if (criteria.Status.HasValue)
            query = query.Where(x => x.Status == criteria.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.Name)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(x => new PriceListSummaryDto(
                x.Id,
                x.StoreId,
                x.Name,
                x.Currency.Code,
                x.Priority,
                x.IsDefault,
                x.Status,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return new PagedResult<PriceListSummaryDto>(
            items,
            criteria.PageNumber,
            criteria.PageSize,
            totalCount);
    }
}
