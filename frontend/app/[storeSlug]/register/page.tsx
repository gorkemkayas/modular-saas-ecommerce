import { RegisterPageContent } from "@/components/auth/register-page-content"
import { storefrontPath } from "@/lib/config"
import { getStorefront } from "@/lib/api/storefront"
import { getTenantBySlug } from "@/lib/tenant"

interface StoreRegisterPageProps {
  params: Promise<{ storeSlug: string }>
}

export default async function StoreRegisterPage({
  params,
}: StoreRegisterPageProps) {
  const { storeSlug } = await params
  const [tenant, storefront] = await Promise.all([
    getTenantBySlug(storeSlug),
    getStorefront(storeSlug),
  ])

  return (
    <RegisterPageContent
      storeSlug={storeSlug}
      storeName={tenant?.name}
      registerPageImageUrl={storefront.registerPageImageUrl}
      nextPath={storefrontPath(storeSlug, "/account")}
    />
  )
}
