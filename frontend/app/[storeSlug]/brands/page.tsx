import Link from "next/link"
import { getStorefrontBrands } from "@/lib/api/storefront"

export default async function StoreBrandsPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const brands = await getStorefrontBrands(storeSlug)

  return (
    <div className="mx-auto max-w-7xl px-6 py-12 lg:py-16">
      <section className="border-b border-border pb-10 lg:pb-12">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="mb-4 text-[11px] uppercase tracking-[0.32em] text-muted-foreground">
              Storefront Brands
            </p>
            <h1 className="font-serif text-4xl font-light tracking-[-0.04em] lg:text-6xl">
              Browse brands.
            </h1>
          </div>

          <div className="max-w-md">
            <p className="text-sm leading-7 text-muted-foreground">
              A clean brand directory with compact boxes and direct access to each label.
            </p>
            <p className="mt-4 text-[11px] uppercase tracking-[0.28em] text-muted-foreground">
              {brands.length} active brands
            </p>
          </div>
        </div>
      </section>

      {brands.length ? (
        <section className="py-10 lg:py-12">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {brands.map((brand) => (
              <article
                key={brand.id}
                className="group border border-border bg-background p-5 transition-all duration-500 hover:border-foreground/30 hover:bg-secondary/20"
              >
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="text-[10px] uppercase tracking-[0.24em] text-muted-foreground">
                      Brand
                    </p>
                    <h2 className="mt-3 font-serif text-[26px] font-light tracking-[-0.03em]">
                      {brand.name}
                    </h2>
                  </div>
                  <div className="border border-border bg-[linear-gradient(180deg,rgba(16,16,16,0.03),rgba(16,16,16,0.08))] px-3 py-2 text-right transition-colors duration-500 group-hover:border-foreground/20">
                    <span className="block text-[10px] uppercase tracking-[0.24em] text-muted-foreground">
                      Products
                    </span>
                    <span className="mt-1 block font-serif text-2xl font-light tracking-[-0.03em] text-foreground">
                      {brand.productCount}
                    </span>
                  </div>
                </div>

                <p className="mt-4 text-sm leading-6 text-muted-foreground">
                  {brand.description ||
                    "Open this brand to view its published products and current storefront assortment."}
                </p>

                <div className="mt-6 flex flex-wrap gap-x-5 gap-y-2 border-t border-border pt-4 text-sm">
                  <Link
                    href={`/${storeSlug}/brands/${brand.slug}`}
                    className="text-foreground underline underline-offset-4"
                  >
                    View brand
                  </Link>
                  <Link
                    href={`/${storeSlug}/products?brandId=${brand.id}`}
                    className="text-muted-foreground underline underline-offset-4 transition-colors hover:text-foreground"
                  >
                    Filter products
                  </Link>
                </div>
              </article>
            ))}
          </div>
        </section>
      ) : (
        <div className="mt-12 border border-border p-10 text-center text-muted-foreground">
          No active brands are available for this storefront yet.
        </div>
      )}
    </div>
  )
}
