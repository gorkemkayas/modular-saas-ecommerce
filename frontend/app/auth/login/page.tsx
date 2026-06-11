import { LoginPageContent } from "@/components/auth/login-page-content"
import { getStorefront } from "@/lib/api/storefront"
import { getTenantBySlug } from "@/lib/tenant"

interface LoginPageProps {
  searchParams?: Promise<{
    storeSlug?: string
    next?: string
    intent?: string
  }>
}

export default async function LoginPage({ searchParams }: LoginPageProps) {
  const resolvedSearchParams = searchParams ? await searchParams : undefined
  const storeSlug = resolvedSearchParams?.storeSlug
  const nextPath = resolvedSearchParams?.next || "/account"
  const isAdminIntent =
    resolvedSearchParams?.intent === "admin" ||
    (storeSlug
      ? nextPath === `/${storeSlug}/admin` || nextPath.startsWith(`/${storeSlug}/admin/`)
      : false)
  const tenant = resolvedSearchParams?.storeSlug
    ? await getTenantBySlug(resolvedSearchParams.storeSlug)
    : null
  const storefront = resolvedSearchParams?.storeSlug && !isAdminIntent
    ? await getStorefront(resolvedSearchParams.storeSlug)
    : null

  return (
    <LoginPageContent
      storeSlug={storeSlug}
      storeName={tenant?.name}
      loginPageImageUrl={storefront?.loginPageImageUrl}
      nextPath={nextPath}
      allowInactiveStore={isAdminIntent}
    />
  )
}
