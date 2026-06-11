import { RelatedProducts } from "@/components/product/related-products"
import { ProductDetail } from "@/components/product/product-detail"
import { getStorefrontProductBySlug, getStorefrontProducts } from "@/lib/api/storefront"
import type { StorefrontProductDto, StorefrontProductSummaryDto } from "@/lib/api/types"

const RELATED_PRODUCTS_LIMIT = 4

async function getRelatedProducts(
  storeSlug: string,
  product: StorefrontProductDto,
): Promise<StorefrontProductSummaryDto[]> {
  const relatedProducts: StorefrontProductSummaryDto[] = []
  const seenProductIds = new Set([product.id])

  function appendProducts(products: StorefrontProductSummaryDto[]) {
    for (const item of products) {
      if (seenProductIds.has(item.id)) {
        continue
      }

      seenProductIds.add(item.id)
      relatedProducts.push(item)

      if (relatedProducts.length >= RELATED_PRODUCTS_LIMIT) {
        return
      }
    }
  }

  const categoryId = product.categories[0]?.categoryId
  if (categoryId) {
    const categoryProducts = await getStorefrontProducts(storeSlug, {
      categoryId,
      pageNumber: 1,
      pageSize: 8,
    })

    appendProducts(categoryProducts.items)
  }

  if (relatedProducts.length < RELATED_PRODUCTS_LIMIT && product.brandId) {
    const brandProducts = await getStorefrontProducts(storeSlug, {
      brandId: product.brandId,
      pageNumber: 1,
      pageSize: 8,
    })

    appendProducts(brandProducts.items)
  }

  if (relatedProducts.length < RELATED_PRODUCTS_LIMIT) {
    const catalogProducts = await getStorefrontProducts(storeSlug, {
      pageNumber: 1,
      pageSize: 12,
    })

    appendProducts(catalogProducts.items)
  }

  return relatedProducts
}

export default async function StoreProductDetailPage({
  params,
}: {
  params: Promise<{ storeSlug: string; slug: string }>
}) {
  const { storeSlug, slug } = await params
  const product = await getStorefrontProductBySlug(storeSlug, slug)
  const relatedProducts = await getRelatedProducts(storeSlug, product)

  return (
    <>
      <ProductDetail product={product} storeSlug={storeSlug} />
      <RelatedProducts
        products={relatedProducts}
        storeSlug={storeSlug}
      />
    </>
  )
}
