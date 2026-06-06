import { ApiError, fetchJson } from "@/lib/api/client"

export interface Tenant {
  id: string
  tenantId: string
  slug: string
  name: string
  logo?: string
  description?: string
  status: "active" | "suspended" | "inactive"
  isPublished: boolean
}

interface TenantStatusResponse {
  id: string
  tenantId: string
  slug: string
  name: string
  description?: string | null
  logoUrl?: string | null
  status: string
  isPublished: boolean
}

export const RESERVED_SLUGS = [
  "admin", "api", "auth", "account", "cart", "checkout",
  "products", "categories", "brands", "contact", "about",
  "help", "faq", "privacy-policy", "terms", "shipping-policy",
  "return-policy", "store-unavailable", "payment-result", "order-success",
]

export function isReservedSlug(slug: string): boolean {
  return RESERVED_SLUGS.includes(slug.toLowerCase())
}

export async function getTenantBySlug(slug: string): Promise<Tenant | null> {
  try {
    const response = await fetchJson<TenantStatusResponse>(`/api/storefront/status/${slug}`)
    return mapTenant(response)
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }

    throw error
  }
}

export async function validateTenant(slug: string): Promise<{
  valid: boolean
  tenant: Tenant | null
  error?: "not_found" | "suspended" | "reserved"
}> {
  if (isReservedSlug(slug)) {
    return { valid: false, tenant: null, error: "reserved" }
  }

  const tenant = await getTenantBySlug(slug)

  if (!tenant) {
    return { valid: false, tenant: null, error: "not_found" }
  }

  if (!tenant.isPublished || tenant.status !== "active") {
    return { valid: false, tenant, error: "suspended" }
  }

  return { valid: true, tenant }
}

function mapTenant(response: TenantStatusResponse): Tenant {
  const normalizedStatus = response.status.toLowerCase()

  return {
    id: response.id,
    tenantId: response.tenantId,
    slug: response.slug,
    name: response.name,
    logo: response.logoUrl ?? undefined,
    description: response.description ?? undefined,
    status:
      normalizedStatus === "active"
        ? "active"
        : normalizedStatus === "suspended"
          ? "suspended"
          : "inactive",
    isPublished: response.isPublished,
  }
}
