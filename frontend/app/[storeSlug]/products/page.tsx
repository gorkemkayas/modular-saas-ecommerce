import Link from "next/link"
import { getStorefrontBrands, getStorefrontCategoryTree, getStorefrontProducts } from "@/lib/api/storefront"
import { ProductCard } from "@/components/product-card"
import { flattenCategories } from "@/lib/storefront-adapters"

export default async function StoreProductsPage({
  params,
  searchParams,
}: {
  params: Promise<{ storeSlug: string }>
  searchParams: Promise<{
    searchTerm?: string
    categoryId?: string
    brandId?: string
    pageNumber?: string
  }>
}) {
  const { storeSlug } = await params
  const query = await searchParams

  const pageNumber = Number(query.pageNumber ?? "1")

  const [categories, brands, products] = await Promise.all([
    getStorefrontCategoryTree(storeSlug),
    getStorefrontBrands(storeSlug),
    getStorefrontProducts(storeSlug, {
      searchTerm: query.searchTerm,
      categoryId: query.categoryId,
      brandId: query.brandId,
      pageNumber,
      pageSize: 20,
    }),
  ])

  const flatCategories = flattenCategories(categories)

  return (
    <div className="mx-auto max-w-7xl px-6 py-12">
      <section className="border-b border-border pb-12">
        <p className="text-xs tracking-[0.3em] text-muted-foreground uppercase mb-4">
          Storefront Catalog
        </p>
        <h1 className="font-serif text-4xl lg:text-6xl font-light tracking-wide">
          Published Products
        </h1>
        <p className="mt-4 text-muted-foreground max-w-xl">
          Results are pulled from the active store catalog, filtered through the backend storefront API.
        </p>
      </section>

      <div className="mt-12 flex gap-12">
        <aside className="hidden w-64 shrink-0 lg:block">
          <div className="sticky top-28 space-y-10">
            <div>
              <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Categories</h3>
              <div className="space-y-3">
                <Link href={`/${storeSlug}/products`} className="block text-sm tracking-wide text-foreground">
                  All Categories
                </Link>
                {flatCategories.map((category) => (
                  <Link
                    key={category.id}
                    href={`/${storeSlug}/products?categoryId=${category.id}`}
                    className="block text-sm tracking-wide text-muted-foreground transition-colors hover:text-foreground"
                  >
                    {category.name}
                  </Link>
                ))}
              </div>
            </div>

            <div>
              <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Brands</h3>
              <div className="space-y-3">
                <Link href={`/${storeSlug}/products`} className="block text-sm tracking-wide text-foreground">
                  All Brands
                </Link>
                {brands.map((brand) => (
                  <Link
                    key={brand.id}
                    href={`/${storeSlug}/products?brandId=${brand.id}`}
                    className="block text-sm tracking-wide text-muted-foreground transition-colors hover:text-foreground"
                  >
                    {brand.name}
                  </Link>
                ))}
              </div>
            </div>
          </div>
        </aside>

        <div className="flex-1">
          <form className="mb-8 flex flex-col gap-4 sm:flex-row">
            <input
              type="text"
              name="searchTerm"
              defaultValue={query.searchTerm ?? ""}
              placeholder="Search products..."
              className="h-12 flex-1 bg-secondary px-4 text-sm"
            />
            {query.categoryId ? <input type="hidden" name="categoryId" value={query.categoryId} /> : null}
            {query.brandId ? <input type="hidden" name="brandId" value={query.brandId} /> : null}
            <button className="h-12 px-6 bg-foreground text-background text-sm uppercase tracking-[0.2em]">
              Search
            </button>
          </form>

          <p className="mb-8 text-sm text-muted-foreground">
            {products.totalCount} published product(s)
          </p>

          {products.items.length ? (
            <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
              {products.items.map((product) => (
                <ProductCard
                  key={product.id}
                  product={product}
                  storeSlug={storeSlug}
                />
              ))}
            </div>
          ) : (
            <div className="border border-border p-10 text-center text-muted-foreground">
              No products matched the current storefront filters.
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
