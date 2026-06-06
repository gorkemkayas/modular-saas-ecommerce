import type { StorefrontProductSummaryDto } from "@/lib/api/types"
import { ProductCard } from "@/components/product-card"

interface RelatedProductsProps {
  products: StorefrontProductSummaryDto[]
  storeSlug: string
}

export function RelatedProducts({
  products,
  storeSlug,
}: RelatedProductsProps) {
  if (!products.length) {
    return null
  }

  return (
    <section className="mt-24 border-t border-border pt-24 lg:mt-32 lg:pt-32">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="flex flex-col items-center text-center">
          <p className="text-xs font-medium uppercase tracking-[0.3em] text-muted-foreground">
            Storefront
          </p>
          <h2 className="mt-4 font-serif text-3xl font-light tracking-tight text-foreground sm:text-4xl">
            Similar Products
          </h2>
        </div>
        <div className="mt-16 grid gap-x-6 gap-y-12 sm:grid-cols-2 lg:grid-cols-4">
          {products.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              storeSlug={storeSlug}
            />
          ))}
        </div>
      </div>
    </section>
  )
}
