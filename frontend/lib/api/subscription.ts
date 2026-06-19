import { ApiError, fetchJson } from "@/lib/api/client"

export const subscriptionFeatureKeys = {
  variantProducts: "catalog.variant-products",
  storefrontVideoHero: "storefront.video-hero",
} as const

export const subscriptionQuotaKeys = {
  catalogProducts: "catalog.products",
  catalogCategories: "catalog.categories",
  catalogMediaPerProduct: "catalog.media-per-product",
  pricingPriceLists: "pricing.price-lists",
  shippingCarriers: "shipment.shipping-carriers",
} as const

export type SubscriptionFeatureKey =
  (typeof subscriptionFeatureKeys)[keyof typeof subscriptionFeatureKeys]

export type SubscriptionQuotaKey =
  (typeof subscriptionQuotaKeys)[keyof typeof subscriptionQuotaKeys]

export interface PlanFeatureDto {
  key: string
  isEnabled: boolean
  description: string | null
}

export interface PlanQuotaDto {
  key: string
  limit: number | null
}

export interface SubscriptionPlanDto {
  code: string
  name: string
  description: string | null
  displayOrder: number
  monthlyPriceAmount: number
  currency: string
  features: PlanFeatureDto[]
  quotas: PlanQuotaDto[]
}

export function formatPlanPrice(plan: SubscriptionPlanDto): string {
  return new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency: plan.currency || "TRY",
    minimumFractionDigits: 2,
  }).format(plan.monthlyPriceAmount)
}

export interface TenantSubscriptionDto {
  subscriptionId: string
  tenantId: string
  planCode: string
  planName: string
  status: string
  startedAtUtc: string
  features: PlanFeatureDto[]
  quotas: PlanQuotaDto[]
}

export async function getPublicPlans(): Promise<SubscriptionPlanDto[]> {
  return fetchJson<SubscriptionPlanDto[]>("/api/plans")
}

export async function getCurrentSubscription(): Promise<TenantSubscriptionDto> {
  return fetchJson<TenantSubscriptionDto>("/api/subscription/me")
}

export async function getCurrentSubscriptionOrNull(): Promise<TenantSubscriptionDto | null> {
  try {
    return await getCurrentSubscription()
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }

    throw error
  }
}

export function hasSubscriptionFeature(
  subscription: Pick<TenantSubscriptionDto, "features"> | null | undefined,
  featureKey: SubscriptionFeatureKey,
): boolean {
  return subscription?.features.some(
    (feature) => feature.key === featureKey && feature.isEnabled,
  ) ?? false
}

export function getSubscriptionQuotaLimit(
  subscription: Pick<TenantSubscriptionDto, "quotas"> | null | undefined,
  quotaKey: SubscriptionQuotaKey,
): number | null | undefined {
  return subscription?.quotas.find((quota) => quota.key === quotaKey)?.limit
}

export function isSubscriptionQuotaReached(
  subscription: Pick<TenantSubscriptionDto, "quotas"> | null | undefined,
  quotaKey: SubscriptionQuotaKey,
  currentCount: number,
): boolean {
  const limit = getSubscriptionQuotaLimit(subscription, quotaKey)
  return typeof limit === "number" && currentCount >= limit
}

export function formatSubscriptionLimit(limit: number | null | undefined): string {
  if (limit === null) {
    return "Unlimited"
  }

  if (typeof limit === "number") {
    return limit.toLocaleString("en-US")
  }

  return "Not configured"
}

export function getSubscriptionFeatureLabel(featureKey: string): string {
  switch (featureKey) {
    case subscriptionFeatureKeys.variantProducts:
      return "Variant products"
    case subscriptionFeatureKeys.storefrontVideoHero:
      return "Storefront video hero"
    default:
      return featureKey
  }
}

export function getSubscriptionQuotaLabel(quotaKey: string): string {
  switch (quotaKey) {
    case subscriptionQuotaKeys.catalogProducts:
      return "Products"
    case subscriptionQuotaKeys.catalogCategories:
      return "Categories"
    case subscriptionQuotaKeys.catalogMediaPerProduct:
      return "Product media"
    case subscriptionQuotaKeys.pricingPriceLists:
      return "Price lists"
    case subscriptionQuotaKeys.shippingCarriers:
      return "Shipping carriers"
    default:
      return quotaKey
  }
}
