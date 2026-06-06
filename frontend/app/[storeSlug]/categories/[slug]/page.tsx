import Link from "next/link"
import { getStorefrontCategoryTree, getStorefrontProducts } from "@/lib/api/storefront"
import { ProductCard } from "@/components/product-card"
import { flattenCategories } from "@/lib/storefront-adapters"

export default async function StoreCategoryPage({
  params,
}: {
  params: Promise<{ storeSlug: string; slug: string }>
}) {
  const { storeSlug, slug } = await params
  const categories = await getStorefrontCategoryTree(storeSlug)
  const flatCategories = flattenCategories(categories)
  const category = flatCategories.find((item) => item.slug === slug)

  if (!category) {
    return (
      <div className="mx-auto max-w-5xl px-6 py-20">
        <h1 className="font-serif text-4xl font-light">Category not found</h1>
      </div>
    )
  }

  const products = await getStorefrontProducts(storeSlug, {
    categoryId: category.id,
    pageNumber: 1,
    pageSize: 20,
  })

  return (
    <div className="mx-auto max-w-7xl px-6 py-12">
      <div className="border-b border-border pb-10">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">
          Catalog Category
        </p>
        <h1 className="mt-4 font-serif text-4xl font-light">{category.name}</h1>
        <p className="mt-4 max-w-2xl text-muted-foreground">
          {category.description || "Published category landing page backed by the storefront catalog API."}
        </p>
        <Link href={`/${storeSlug}/products?categoryId=${category.id}`} className="mt-6 inline-block text-sm underline underline-offset-4">
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
