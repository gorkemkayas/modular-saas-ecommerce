import type { Metadata } from "next"

import { StoreOwnerRegisterPageContent } from "@/components/auth/store-owner-register-page-content"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { getPublicPlans } from "@/lib/api/subscription"

export const dynamic = "force-dynamic"

export const metadata: Metadata = {
  title: "Create Store | KAYAS",
  description: "Register a new store owner account and choose a subscription plan.",
}

interface StoreRegisterPageProps {
  searchParams?: Promise<{
    plan?: string
  }>
}

export default async function StoreRegisterPage({
  searchParams,
}: StoreRegisterPageProps) {
  const resolvedSearchParams = searchParams ? await searchParams : undefined
  const initialPlanCode = resolvedSearchParams?.plan?.trim().toLowerCase()

  try {
    const plans = await getPublicPlans()

    return (
      <StoreOwnerRegisterPageContent
        plans={plans}
        initialPlanCode={initialPlanCode}
      />
    )
  } catch (error) {
    return (
      <StoreOwnerRegisterPageContent
        plans={[]}
        initialPlanCode={initialPlanCode}
        planLoadError={getApiErrorMessage(
          error,
          "Subscription plans could not be loaded.",
        )}
      />
    )
  }
}
