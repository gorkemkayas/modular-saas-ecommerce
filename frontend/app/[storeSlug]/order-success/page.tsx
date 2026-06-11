import { Suspense } from "react"
import { StoreOrderSuccessContent } from "@/components/storefront/store-order-success-content"

export default function StoreOrderSuccessPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  return (
    <Suspense fallback={null}>
      <StoreOrderSuccessContent params={params} />
    </Suspense>
  )
}
