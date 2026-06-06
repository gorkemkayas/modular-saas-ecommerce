import { redirect } from "next/navigation"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"

export default async function OrderSuccessRedirect({
  searchParams,
}: {
  searchParams: Promise<{ orderId?: string }>
}) {
  const resolvedSearchParams = await searchParams

  if (defaultStoreSlug) {
    const target = storefrontPath(
      defaultStoreSlug,
      `/order-success${resolvedSearchParams.orderId ? `?orderId=${encodeURIComponent(resolvedSearchParams.orderId)}` : ""}`,
    )
    redirect(target)
  }

  redirect("/")
}
