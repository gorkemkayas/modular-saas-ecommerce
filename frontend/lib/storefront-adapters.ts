import type {
  StorefrontCategoryTreeNodeDto,
  StorefrontProductDto,
  StorefrontProductSummaryDto,
} from "@/lib/api/types"

export interface CatalogCategoryItem {
  id: string
  name: string
  slug: string
  description: string | null
  productCount?: number
}

export interface CartProductInput {
  productId: string
  productSlug: string
  name: string
  brandName: string | null
  categoryName: string | null
  imageUrl: string | null
  priceAmount: number
  currencyCode: string
  compareAtAmount: number | null
  variantId: string | null
  variantName: string | null
  selectedOptions: string[]
}

export function flattenCategories(
  categories: StorefrontCategoryTreeNodeDto[],
): CatalogCategoryItem[] {
  const result: CatalogCategoryItem[] = []

  const visit = (node: StorefrontCategoryTreeNodeDto): void => {
    result.push({
      id: node.id,
      name: node.name,
      slug: node.slug,
      description: node.description,
    })

    node.children.forEach(visit)
  }

  categories.forEach(visit)
  return result
}

export function toCartProductFromSummary(
  product: StorefrontProductSummaryDto,
): CartProductInput | null {
  if (!product.price) {
    return null
  }

  return {
    productId: product.id,
    productSlug: product.slug,
    name: product.name,
    brandName: product.brandName,
    categoryName: null,
    imageUrl: product.mainImageUrl,
    priceAmount: product.price.amount,
    currencyCode: product.price.currencyCode,
    compareAtAmount: product.price.compareAtAmount,
    variantId: product.price.productVariantId,
    variantName: null,
    selectedOptions: [],
  }
}

export function toCartProductFromDetail(
  product: StorefrontProductDto,
  variantId?: string | null,
  preferredImageUrl?: string | null,
): CartProductInput | null {
  const variant =
    variantId ? product.variants.find((item) => item.id === variantId) ?? null : null
  const price = variant?.price ?? product.price

  if (!price) {
    return null
  }

  const imageUrl =
    preferredImageUrl ??
    variant?.mediaItems.find((item) => item.isMain)?.url ??
    variant?.mediaItems[0]?.url ??
    product.mediaItems.find((item) => item.isMain)?.url ??
    product.mediaItems[0]?.url ??
    null

  return {
    productId: product.id,
    productSlug: product.slug,
    name: product.name,
    brandName: product.brandName,
    categoryName: product.categories[0]?.name ?? null,
    imageUrl,
    priceAmount: price.amount,
    currencyCode: price.currencyCode,
    compareAtAmount: price.compareAtAmount,
    variantId: variant?.id ?? price.productVariantId,
    variantName: variant?.name ?? null,
    selectedOptions: variant?.attributes.map((item) => `${item.name}: ${item.value}`) ?? [],
  }
}
