import { redirect } from "next/navigation"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"

export default async function PaymentResultRedirect({
  searchParams,
}: {
  searchParams: Promise<{ status?: string; orderId?: string }>
}) {
  const resolvedSearchParams = await searchParams

  if (defaultStoreSlug) {
    const query = new URLSearchParams()

    if (resolvedSearchParams.status) {
      query.set("status", resolvedSearchParams.status)
    }

    if (resolvedSearchParams.orderId) {
      query.set("orderId", resolvedSearchParams.orderId)
    }

    const suffix = query.toString() ? `?${query.toString()}` : ""
    redirect(storefrontPath(defaultStoreSlug, `/payment-result${suffix}`))
  }

  redirect("/")
}
