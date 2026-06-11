import { CheckoutContent } from "@/components/checkout/checkout-content"

export default async function StoreCheckoutPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  return <CheckoutContent storeSlug={storeSlug} />
}
