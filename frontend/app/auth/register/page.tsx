import { RegisterPageContent } from "@/components/auth/register-page-content"
import { getStorefront } from "@/lib/api/storefront"
import { getTenantBySlug } from "@/lib/tenant"

interface RegisterPageProps {
  searchParams?: Promise<{
    storeSlug?: string
    next?: string
  }>
}

export default async function RegisterPage({
  searchParams,
}: RegisterPageProps) {
  const resolvedSearchParams = searchParams ? await searchParams : undefined
  const tenant = resolvedSearchParams?.storeSlug
    ? await getTenantBySlug(resolvedSearchParams.storeSlug)
    : null
  const storefront = resolvedSearchParams?.storeSlug
    ? await getStorefront(resolvedSearchParams.storeSlug)
    : null

  return (
    <RegisterPageContent
      storeSlug={resolvedSearchParams?.storeSlug}
      storeName={tenant?.name}
      registerPageImageUrl={storefront?.registerPageImageUrl}
      nextPath={resolvedSearchParams?.next || "/account"}
    />
  )
}
