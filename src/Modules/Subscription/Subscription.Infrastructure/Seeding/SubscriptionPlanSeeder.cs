using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Subscription.Contracts;
using Subscription.Domain.Entities;
using Subscription.Infrastructure.Persistence;

namespace Subscription.Infrastructure.Seeding;

public sealed class SubscriptionPlanSeeder
{
    private readonly SubscriptionDbContext _context;
    private readonly ILogger<SubscriptionPlanSeeder> _logger;

    public SubscriptionPlanSeeder(
        SubscriptionDbContext context,
        ILogger<SubscriptionPlanSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var definition in GetDefinitions())
        {
            var plan = await _context.Plans
                .Include(x => x.Features)
                .Include(x => x.Quotas)
                .FirstOrDefaultAsync(x => x.Code == definition.Code, cancellationToken);

            if (plan is null)
            {
                plan = Plan.Create(
                    definition.Code,
                    definition.Name,
                    definition.Description,
                    definition.DisplayOrder,
                    definition.MonthlyPriceAmount,
                    definition.Currency);

                await _context.Plans.AddAsync(plan, cancellationToken);

                _logger.LogInformation(
                    "Seeded subscription plan | PlanCode: {PlanCode}",
                    definition.Code);
            }
            else
            {
                plan.UpdateMetadata(
                    definition.Name,
                    definition.Description,
                    definition.DisplayOrder,
                    isPublic: true,
                    definition.MonthlyPriceAmount,
                    definition.Currency);

                plan.Activate();
            }

            foreach (var feature in definition.Features)
            {
                plan.UpsertFeature(feature.Key, feature.IsEnabled, feature.Description);
            }

            foreach (var quota in definition.Quotas)
            {
                plan.UpsertQuota(quota.Key, quota.Limit);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<SubscriptionPlanDefinition> GetDefinitions()
    {
        return
        [
            new(
                SubscriptionPlanCodes.Starter,
                "Starter",
                "Launch a small store with essential commerce features.",
                10,
                99.99m,
                "TRY",
                [
                    new(SubscriptionFeatureKeys.VariantProducts, false, "Variant products are not included."),
                    new(SubscriptionFeatureKeys.AdvancedNotifications, false, "Custom notification templates are not included."),
                    new(SubscriptionFeatureKeys.StorefrontVideoHero, false, "Video hero media is not included."),
                    new(SubscriptionFeatureKeys.CustomDomain, false, "Custom storefront domains are not included."),
                    new(SubscriptionFeatureKeys.OrderExport, false, "Order export is not included."),
                    new(SubscriptionFeatureKeys.ApiAccess, false, "External API access is not included.")
                ],
                [
                    new(SubscriptionQuotaKeys.CatalogProducts, 50),
                    new(SubscriptionQuotaKeys.CatalogCategories, 10),
                    new(SubscriptionQuotaKeys.CatalogMediaPerProduct, 3),
                    new(SubscriptionQuotaKeys.PricingPriceLists, 1),
                    new(SubscriptionQuotaKeys.ShippingCarriers, 1),
                    new(SubscriptionQuotaKeys.StaffUsers, 1),
                    new(SubscriptionQuotaKeys.MediaStorageMb, 256)
                ]),
            new(
                SubscriptionPlanCodes.Growth,
                "Growth",
                "Scale a growing catalog with richer storefront and operations tools.",
                20,
                249.99m,
                "TRY",
                [
                    new(SubscriptionFeatureKeys.VariantProducts, true, "Variant products are included."),
                    new(SubscriptionFeatureKeys.AdvancedNotifications, true, "Custom notification templates are included."),
                    new(SubscriptionFeatureKeys.StorefrontVideoHero, true, "Video hero media is included."),
                    new(SubscriptionFeatureKeys.CustomDomain, false, "Custom storefront domains are not included."),
                    new(SubscriptionFeatureKeys.OrderExport, true, "Order export is included."),
                    new(SubscriptionFeatureKeys.ApiAccess, false, "External API access is not included.")
                ],
                [
                    new(SubscriptionQuotaKeys.CatalogProducts, 500),
                    new(SubscriptionQuotaKeys.CatalogCategories, 50),
                    new(SubscriptionQuotaKeys.CatalogMediaPerProduct, 10),
                    new(SubscriptionQuotaKeys.PricingPriceLists, 5),
                    new(SubscriptionQuotaKeys.ShippingCarriers, 3),
                    new(SubscriptionQuotaKeys.StaffUsers, 5),
                    new(SubscriptionQuotaKeys.MediaStorageMb, 2048)
                ]),
            new(
                SubscriptionPlanCodes.Premium,
                "Premium",
                "Unlock advanced commerce features and high operational limits.",
                30,
                499.99m,
                "TRY",
                [
                    new(SubscriptionFeatureKeys.VariantProducts, true, "Variant products are included."),
                    new(SubscriptionFeatureKeys.AdvancedNotifications, true, "Custom notification templates are included."),
                    new(SubscriptionFeatureKeys.StorefrontVideoHero, true, "Video hero media is included."),
                    new(SubscriptionFeatureKeys.CustomDomain, true, "Custom storefront domains are included."),
                    new(SubscriptionFeatureKeys.OrderExport, true, "Order export is included."),
                    new(SubscriptionFeatureKeys.ApiAccess, true, "External API access is included.")
                ],
                [
                    new(SubscriptionQuotaKeys.CatalogProducts, null),
                    new(SubscriptionQuotaKeys.CatalogCategories, null),
                    new(SubscriptionQuotaKeys.CatalogMediaPerProduct, null),
                    new(SubscriptionQuotaKeys.PricingPriceLists, null),
                    new(SubscriptionQuotaKeys.ShippingCarriers, null),
                    new(SubscriptionQuotaKeys.StaffUsers, 20),
                    new(SubscriptionQuotaKeys.MediaStorageMb, 10240)
                ])
        ];
    }

    private sealed record SubscriptionPlanDefinition(
        string Code,
        string Name,
        string Description,
        int DisplayOrder,
        decimal MonthlyPriceAmount,
        string Currency,
        IReadOnlyCollection<FeatureDefinition> Features,
        IReadOnlyCollection<QuotaDefinition> Quotas);

    private sealed record FeatureDefinition(
        string Key,
        bool IsEnabled,
        string Description);

    private sealed record QuotaDefinition(
        string Key,
        int? Limit);
}
