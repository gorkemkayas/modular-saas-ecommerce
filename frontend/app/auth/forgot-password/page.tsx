import { ForgotPasswordPageContent } from "@/components/auth/forgot-password-page-content"

interface ForgotPasswordPageProps {
  searchParams?: Promise<{
    storeSlug?: string
  }>
}

export default async function ForgotPasswordPage({
  searchParams,
}: ForgotPasswordPageProps) {
  const resolvedSearchParams = searchParams ? await searchParams : undefined

  return <ForgotPasswordPageContent storeSlug={resolvedSearchParams?.storeSlug} />
}
