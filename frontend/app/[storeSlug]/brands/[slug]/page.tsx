import Link from "next/link"
import { getStorefrontBrands, getStorefrontProducts } from "@/lib/api/storefront"
import { ProductCard } from "@/components/product-card"

export default async function StoreBrandPage({
  params,
}: {
  params: Promise<{ storeSlug: string; slug: string }>
}) {
  const { storeSlug, slug } = await params
  const brands = await getStorefrontBrands(storeSlug)
  const brand = brands.find((item) => item.slug === slug)

  if (!brand) {
    return (
      <div className="mx-auto max-w-5xl px-6 py-20">
        <h1 className="font-serif text-4xl font-light">Brand not found</h1>
      </div>
    )
  }

  const products = await getStorefrontProducts(storeSlug, {
    brandId: brand.id,
    pageNumber: 1,
    pageSize: 20,
  })

  return (
    <div className="mx-auto max-w-7xl px-6 py-12">
      <div className="border-b border-border pb-10">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">
          Storefront Brand
        </p>
        <h1 className="mt-4 font-serif text-4xl font-light">{brand.name}</h1>
        <p className="mt-4 max-w-2xl text-muted-foreground">
          {brand.description || "Brand landing page backed by the storefront brand search endpoint."}
        </p>
        <p className="mt-2 text-sm text-muted-foreground">{brand.productCount} product(s)</p>
        <Link href={`/${storeSlug}/products?brandId=${brand.id}`} className="mt-6 inline-block text-sm underline underline-offset-4">
          Open filtered catalog view
        </Link>
      </div>

      <div className="mt-12 grid grid-cols-1 gap-8 sm:grid-cols-2 lg:grid-cols-3">
        {products.items.map((product) => (
          <ProductCard key={product.id} product={product} storeSlug={storeSlug} />
        ))}
      </div>
    </div>
  )
}
