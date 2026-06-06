import { ForgotPasswordPageContent } from "@/components/auth/forgot-password-page-content"

interface StoreForgotPasswordPageProps {
  params: Promise<{ storeSlug: string }>
}

export default async function StoreForgotPasswordPage({
  params,
}: StoreForgotPasswordPageProps) {
  const { storeSlug } = await params

  return <ForgotPasswordPageContent storeSlug={storeSlug} />
}
