using Microsoft.EntityFrameworkCore;
using Pricing.Application.Abstractions.Queries;
using Pricing.Contracts;
using Pricing.Domain.Enums;
using Pricing.Domain.ValueObjects;
using Pricing.Infrastructure.Persistence;

namespace Pricing.Infrastructure.ReadServices;

public sealed class PriceCoverageReadService : IPriceCoverageReadService
{
    private readonly PricingDbContext _context;

    public PriceCoverageReadService(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<PriceCoverageResult> CheckCoverageAsync(
        CheckPriceCoverageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.StoreId == Guid.Empty || request.Targets.Count == 0)
            return new PriceCoverageResult(false, request.Targets);

        var distinctTargets = request.Targets
            .Where(x => x.ProductId != Guid.Empty)
            .Distinct()
            .ToArray();

        if (distinctTargets.Length == 0)
            return new PriceCoverageResult(false, request.Targets);

        var query = _context.PriceLists
            .AsNoTracking()
            .Include(x => x.Entries)
            .Where(x =>
                x.StoreId == request.StoreId &&
                x.IsDefault &&
                x.Status == PriceListStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            var currency = Currency.Create(request.CurrencyCode);
            query = query.Where(x => x.Currency == currency);
        }

        var priceLists = await query.ToArrayAsync(cancellationToken);

        if (priceLists.Length == 0)
            return new PriceCoverageResult(false, distinctTargets);

        var missingTargets = distinctTargets
            .Where(target => !priceLists.Any(priceList =>
                priceList.Entries.Any(entry =>
                    entry.IsActive &&
                    entry.Target.ProductId == target.ProductId &&
                    entry.Target.ProductVariantId == target.ProductVariantId)))
            .ToArray();

        return new PriceCoverageResult(missingTargets.Length == 0, missingTargets);
    }
}
