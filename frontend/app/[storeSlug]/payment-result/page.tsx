import { Suspense } from "react"
import { StorePaymentResultContent } from "@/components/storefront/store-payment-result-content"

export default function StorePaymentResultPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  return (
    <Suspense fallback={null}>
      <StorePaymentResultContent params={params} />
    </Suspense>
  )
}
