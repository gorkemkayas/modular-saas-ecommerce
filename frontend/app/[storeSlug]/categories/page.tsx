import Image from "next/image"
import Link from "next/link"
import { getStorefrontCategoryTree } from "@/lib/api/storefront"
import type { StorefrontCategoryTreeNodeDto } from "@/lib/api/types"

interface CategoryListItem {
  id: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  path: string
}

function flattenCategoryTree(
  nodes: StorefrontCategoryTreeNodeDto[],
  ancestors: string[] = [],
): CategoryListItem[] {
  return nodes.flatMap((node) => {
    const pathSegments = [...ancestors, node.name]

    return [
      {
        id: node.id,
        name: node.name,
        slug: node.slug,
        description: node.description,
        imageUrl: node.imageUrl,
        path: pathSegments.join(" / "),
      },
      ...flattenCategoryTree(node.children, pathSegments),
    ]
  })
}

export default async function StoreCategoriesPage({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const categories = await getStorefrontCategoryTree(storeSlug)
  const flatCategories = flattenCategoryTree(categories)

  return (
    <div className="mx-auto max-w-7xl px-6 py-12 lg:py-16">
      <section className="border-b border-border pb-10 lg:pb-12">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="mb-4 text-[11px] uppercase tracking-[0.32em] text-muted-foreground">
              Storefront Categories
            </p>
            <h1 className="font-serif text-4xl font-light tracking-[-0.04em] lg:text-6xl">
              Browse categories.
            </h1>
          </div>

          <div className="max-w-md">
            <p className="text-sm leading-7 text-muted-foreground">
              A compact catalog index built around small, clean category boxes.
            </p>
            <p className="mt-4 text-[11px] uppercase tracking-[0.28em] text-muted-foreground">
              {flatCategories.length} published categories
            </p>
          </div>
        </div>
      </section>

      {flatCategories.length ? (
        <section className="py-10 lg:py-12">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {flatCategories.map((category) => (
              <article
                key={category.id}
                className="group border border-border bg-background transition-all duration-500 hover:border-foreground/30 hover:bg-secondary/20"
              >
                <Link href={`/${storeSlug}/categories/${category.slug}`} className="block">
                  {category.imageUrl ? (
                    <div className="relative aspect-[4/3] overflow-hidden bg-secondary">
                      <Image
                        src={category.imageUrl}
                        alt={category.name}
                        fill
                        className="object-cover transition-transform duration-700 group-hover:scale-[1.04]"
                        sizes="(min-width: 1280px) 33vw, (min-width: 640px) 50vw, 100vw"
                      />
                      <div className="absolute inset-0 bg-gradient-to-t from-black/65 via-black/25 to-black/10 transition-opacity duration-500 group-hover:opacity-80" />
                      <div className="absolute left-4 top-4 border border-white/20 bg-black/25 px-3 py-1.5 backdrop-blur-sm">
                        <span className="text-[10px] uppercase tracking-[0.24em] text-white/80">
                          Category
                        </span>
                      </div>
                      <div className="absolute inset-x-0 bottom-0 p-5">
                        <p className="text-[10px] uppercase tracking-[0.24em] text-white/70">
                          {category.path}
                        </p>
                        <p className="mt-2 font-serif text-2xl font-light tracking-[-0.03em] text-white">
                          {category.name}
                        </p>
                      </div>
                    </div>
                  ) : (
                    <div className="flex aspect-[4/3] items-end bg-[linear-gradient(180deg,rgba(16,16,16,0.08),rgba(16,16,16,0.22))] p-5">
                      <span className="text-[11px] uppercase tracking-[0.25em] text-muted-foreground">
                        Category
                      </span>
                    </div>
                  )}
                </Link>

                <div className="space-y-4 p-5">
                  <div>
                    <p className="text-[10px] uppercase tracking-[0.24em] text-muted-foreground">
                      Published section
                    </p>
                    <h2 className="mt-3 font-serif text-[26px] font-light tracking-[-0.03em]">
                      {category.name}
                    </h2>
                    <p className="mt-3 text-sm leading-6 text-muted-foreground">
                      {category.description ||
                      "Open this category to explore its published assortment."}
                    </p>
                  </div>

                  <div className="flex flex-wrap gap-x-5 gap-y-2 border-t border-border pt-4 text-sm">
                    <Link
                      href={`/${storeSlug}/categories/${category.slug}`}
                      className="text-foreground underline underline-offset-4"
                    >
                      View category
                    </Link>
                    <Link
                      href={`/${storeSlug}/products?categoryId=${category.id}`}
                      className="text-muted-foreground underline underline-offset-4 transition-colors hover:text-foreground"
                    >
                      Filter products
                    </Link>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </section>
      ) : (
        <div className="mt-12 border border-border p-10 text-center text-muted-foreground">
          No published categories are available for this storefront yet.
        </div>
      )}
    </div>
  )
}
