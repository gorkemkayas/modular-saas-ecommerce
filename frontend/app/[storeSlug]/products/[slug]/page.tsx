import { RelatedProducts } from "@/components/product/related-products"
import { ProductDetail } from "@/components/product/product-detail"
import { getStorefrontProductBySlug, getStorefrontProducts } from "@/lib/api/storefront"

export default async function StoreProductDetailPage({
  params,
}: {
  params: Promise<{ storeSlug: string; slug: string }>
}) {
  const { storeSlug, slug } = await params
  const product = await getStorefrontProductBySlug(storeSlug, slug)

  const categoryId = product.categories[0]?.categoryId
  const related = categoryId
    ? await getStorefrontProducts(storeSlug, {
        categoryId,
        pageNumber: 1,
        pageSize: 5,
      })
    : null

  return (
    <>
      <ProductDetail product={product} storeSlug={storeSlug} />
      <RelatedProducts
        products={(related?.items ?? []).filter((item) => item.id !== product.id).slice(0, 4)}
        storeSlug={storeSlug}
      />
    </>
  )
}
