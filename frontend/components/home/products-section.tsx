import Link from "next/link"
import { useMemo } from "react"
import type {
  StorefrontCategoryTreeNodeDto,
  StorefrontProductSummaryDto,
} from "@/lib/api/types"
import { ProductCard } from "@/components/product-card"
import { flattenCategories } from "@/lib/storefront-adapters"
import { cn } from "@/lib/utils"

interface ProductsSectionProps {
  storeSlug: string
  products: StorefrontProductSummaryDto[]
  categories: StorefrontCategoryTreeNodeDto[]
}

export function ProductsSection({
  storeSlug,
  products,
  categories,
}: ProductsSectionProps) {
  const availableCategories = useMemo(() => flattenCategories(categories), [categories])

  return (
    <section id="products" className="bg-secondary/30 py-32 lg:py-40">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mb-16 flex flex-col items-center text-center">
          <p className="text-[10px] font-normal uppercase tracking-[0.4em] text-muted-foreground">
            Published Catalog
          </p>
          <h2 className="mt-4 font-serif text-4xl font-light tracking-tight text-foreground sm:text-5xl lg:text-6xl">
            Full Collection
          </h2>
          <div className="mt-6 h-px w-16 bg-foreground/20" />
        </div>

        <div className="mb-16 flex flex-wrap items-center justify-center gap-1">
          <Link
            href={`/${storeSlug}/products`}
            className={cn(
              "relative px-6 py-3 text-[11px] font-normal uppercase tracking-[0.25em] transition-all duration-300",
              "text-foreground",
            )}
          >
            All
            <span className="absolute bottom-2 left-1/2 h-px w-6 -translate-x-1/2 bg-foreground" />
          </Link>

          {availableCategories.slice(0, 6).map((category) => (
            <Link
              key={category.id}
              href={`/${storeSlug}/products?categoryId=${category.id}`}
              className={cn(
                "relative px-6 py-3 text-[11px] font-normal uppercase tracking-[0.25em] transition-all duration-300",
                "text-muted-foreground hover:text-foreground",
              )}
            >
              {category.name}
            </Link>
          ))}
        </div>

        <div className="grid gap-x-4 gap-y-16 sm:grid-cols-2 lg:grid-cols-4">
          {products.map((product, index) => (
            <div
              key={product.id}
              className="animate-fade-up opacity-0"
              style={{
                animationDelay: `${index * 100}ms`,
                animationFillMode: "forwards",
              }}
            >
              <ProductCard product={product} storeSlug={storeSlug} />
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
