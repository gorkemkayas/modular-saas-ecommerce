import { CategoriesSection } from "@/components/home/categories-section"
import { FeaturesSection } from "@/components/home/features-section"
import { HeroSection } from "@/components/home/hero-section"
import { ProductsSection } from "@/components/home/products-section"
import {
  getStorefront,
  getStorefrontCategoryTree,
  getStorefrontProducts,
} from "@/lib/api/storefront"

export default async function StoreHomePage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const [storefront, categories, products] = await Promise.all([
    getStorefront(storeSlug),
    getStorefrontCategoryTree(storeSlug),
    getStorefrontProducts(storeSlug, { pageNumber: 1, pageSize: 8 }),
  ])

  return (
    <>
      <HeroSection
        storeSlug={storeSlug}
        storeName={storefront.name}
        heroImageUrl={storefront.heroImageUrl}
        heroMediaType={storefront.heroMediaType}
        heroEyebrowText={storefront.heroEyebrowText}
        heroTitle={storefront.heroTitle}
        heroAccentTitle={storefront.heroAccentTitle}
        heroDescription={storefront.heroDescription}
        heroPrimaryButtonText={storefront.heroPrimaryButtonText}
      />
      <CategoriesSection storeSlug={storeSlug} categories={categories} />
      <ProductsSection
        storeSlug={storeSlug}
        categories={categories}
        products={products.items}
      />
      <FeaturesSection />
    </>
  )
}
