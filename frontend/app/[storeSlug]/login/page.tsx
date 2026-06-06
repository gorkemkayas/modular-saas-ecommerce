import { LoginPageContent } from "@/components/auth/login-page-content"
import { storefrontPath } from "@/lib/config"
import { getStorefront } from "@/lib/api/storefront"
import { getTenantBySlug } from "@/lib/tenant"

interface StoreLoginPageProps {
  params: Promise<{ storeSlug: string }>
  searchParams?: Promise<{
    next?: string
  }>
}

export default async function StoreLoginPage({
  params,
  searchParams,
}: StoreLoginPageProps) {
  const { storeSlug } = await params
  const resolvedSearchParams = searchParams ? await searchParams : undefined
  const [tenant, storefront] = await Promise.all([
    getTenantBySlug(storeSlug),
    getStorefront(storeSlug),
  ])

  return (
    <LoginPageContent
      storeSlug={storeSlug}
      storeName={tenant?.name}
      loginPageImageUrl={storefront.loginPageImageUrl}
      nextPath={resolvedSearchParams?.next || storefrontPath(storeSlug, "/account")}
    />
  )
}
