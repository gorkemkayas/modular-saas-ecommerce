import { CartContent } from "@/components/cart/cart-content"

export default async function StoreCartPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  return <CartContent storeSlug={storeSlug} />
}
