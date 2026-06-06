import { headers } from "next/headers"
import { redirect } from "next/navigation"
import type { ReactNode } from "react"
import { StoreNotFound } from "@/components/tenant/store-not-found"
import { StorefrontLayoutShell } from "@/components/storefront/storefront-layout-shell"
import { getAuthSession, isSessionForStore } from "@/lib/api/auth"
import { withQuery } from "@/lib/config"
import { RESERVED_SLUGS, validateTenant } from "@/lib/tenant"

export default async function StorefrontLayout({
  children,
  params,
}: {
  children: ReactNode
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const pathname = (await headers()).get("x-pathname") || `/${storeSlug}`
  let storeName: string | null = null

  if (!RESERVED_SLUGS.includes(storeSlug.toLowerCase())) {
    const { valid, tenant, error } = await validateTenant(storeSlug)
    storeName = tenant?.name ?? null

    if (error === "not_found") {
      return <StoreNotFound slug={storeSlug} />
    }

    if (!valid && error !== "suspended") {
      return <StoreNotFound slug={storeSlug} />
    }
  }

  const session = await getAuthSession()

  if (session.isAuthenticated && !isSessionForStore(session, storeSlug)) {
    redirect(
      withQuery("/auth/logout", {
        redirectTo: pathname,
      }),
    )
  }

  return (
    <StorefrontLayoutShell
      storeSlug={storeSlug}
      storeName={storeName}
      initialSession={{
        isAuthenticated: session.isAuthenticated,
        canAccessAdmin: session.canAccessAdmin,
        storeSlug: session.storeSlug,
      }}
    >
      {children}
    </StorefrontLayoutShell>
  )
}
