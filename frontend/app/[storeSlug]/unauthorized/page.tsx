import { Unauthorized } from "@/components/auth/unauthorized"
import { getTenantBySlug } from "@/lib/tenant"

export default async function StoreUnauthorizedPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const tenant = await getTenantBySlug(storeSlug)

  return <Unauthorized storeSlug={storeSlug} storeName={tenant?.name} />
}
