import Link from "next/link"
import Image from "next/image"
import type { StorefrontCategoryTreeNodeDto } from "@/lib/api/types"
import { storefrontPath } from "@/lib/config"

interface CategoriesSectionProps {
  storeSlug: string
  categories: StorefrontCategoryTreeNodeDto[]
}

export function CategoriesSection({
  storeSlug,
  categories,
}: CategoriesSectionProps) {
  const featuredCategories = categories.slice(0, 4)

  return (
    <section id="categories" className="py-32 lg:py-40">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="mb-20 flex flex-col lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="text-[10px] font-normal uppercase tracking-[0.4em] text-muted-foreground">
              Store Structure
            </p>
            <h2 className="mt-4 font-serif text-4xl font-light tracking-tight text-foreground sm:text-5xl lg:text-6xl">
              Categories
            </h2>
          </div>
          <p className="mt-6 max-w-md text-sm leading-relaxed text-muted-foreground lg:mt-0 lg:text-right">
            Published categories come directly from the catalog module and drive the
            storefront navigation.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {featuredCategories.map((category, index) => (
            <Link
              key={category.id}
              href={storefrontPath(storeSlug, `/categories/${category.slug}`)}
              className="group relative overflow-hidden"
            >
              <div className="relative aspect-[3/4] overflow-hidden bg-secondary">
                <Image
                  src={category.imageUrl || "/placeholder.jpg"}
                  alt={category.name}
                  fill
                  className="object-cover transition-all duration-1000 ease-out group-hover:scale-110"
                  sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 25vw"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/20 to-transparent" />

                <span className="absolute right-4 top-4 font-serif text-5xl font-light text-white/20">
                  0{index + 1}
                </span>
              </div>

              <div className="absolute inset-x-0 bottom-0 p-6 lg:p-8">
                <p className="text-[10px] font-normal uppercase tracking-[0.3em] text-white/60 transition-colors duration-300 group-hover:text-white/80">
                  {category.description || "Published category"}
                </p>
                <h3 className="mt-2 font-serif text-2xl font-light tracking-wide text-white lg:text-3xl">
                  {category.name}
                </h3>
                <div className="mt-4 h-px w-0 bg-white/50 transition-all duration-500 group-hover:w-full" />
              </div>
            </Link>
          ))}
        </div>
      </div>
    </section>
  )
}
