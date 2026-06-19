import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminProductLifecycleActions } from "@/components/admin/admin-product-lifecycle-actions"
import { getAttributeDefinitionById, getBrandById, getCategoryTree, getProductById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

function flattenCategories(
  nodes: Awaited<ReturnType<typeof getCategoryTree>>,
): Map<string, string> {
  const entries = new Map<string, string>()

  const visit = (currentNodes: typeof nodes) => {
    for (const node of currentNodes) {
      entries.set(node.id, node.name)
      visit(node.children)
    }
  }

  visit(nodes)
  return entries
}

function getLifecycleClasses(status: string): string {
  switch (status) {
    case "Active":
      return "bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
    case "Archived":
      return "bg-muted text-muted-foreground"
    default:
      return "bg-amber-500/10 text-amber-700 dark:text-amber-300"
  }
}

export default async function AdminProductDetailPage({ params }: { params: { id: string } }) {
  try {
    const product = await getProductById(params.id)
    const categoryIds = product.categories.map((category) => category.categoryId)
    const attributeDefinitionIds = [
      ...new Set(product.attributeValues.map((attribute) => attribute.attributeDefinitionId)),
    ]

    const [categoryTree, brand, attributeDefinitions] = await Promise.all([
      getCategoryTree(),
      product.brandId ? getBrandById(product.brandId) : Promise.resolve(null),
      Promise.all(
        attributeDefinitionIds.map((attributeDefinitionId) =>
          getAttributeDefinitionById(attributeDefinitionId),
        ),
      ),
    ])

    const categoryMap = flattenCategories(categoryTree)
    const attributeMap = new Map(
      attributeDefinitions.map((attributeDefinition) => [
        attributeDefinition.id,
        attributeDefinition.name,
      ]),
    )

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/products" className="hover:text-foreground">
            Products
          </Link>
          <span>/</span>
          <span className="text-foreground">{product.name}</span>
        </nav>

        <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
          <div className="space-y-8">
            <div className="border border-border bg-gradient-to-br from-background via-background to-secondary/40 p-6">
              <div className="space-y-4">
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className={`px-3 py-1 text-xs font-medium ${getLifecycleClasses(
                      product.productStatus,
                    )}`}
                  >
                    {formatEnumLabel(product.productStatus)}
                  </span>
                  <span className="bg-secondary px-3 py-1 text-xs font-medium text-foreground">
                    {formatEnumLabel(product.productType)}
                  </span>
                  <span
                    className={`px-3 py-1 text-xs font-medium ${
                      product.isPublished
                        ? "bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
                        : "bg-muted text-muted-foreground"
                    }`}
                  >
                    {product.isPublished ? "Published" : "Private"}
                  </span>
                </div>
                <div>
                  <h1 className="text-3xl font-medium tracking-tight">{product.name}</h1>
                  <p className="mt-2 text-sm text-muted-foreground">/{product.slug}</p>
                </div>
              </div>
              {product.shortDescription ? (
                <p className="mt-4 max-w-2xl text-sm leading-6 text-muted-foreground">
                  {product.shortDescription}
                </p>
              ) : null}
            </div>

            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {product.mediaItems.length ? (
                product.mediaItems.map((media, index) => (
                  <div
                    key={media.id}
                    className="overflow-hidden border border-border bg-background/80"
                  >
                    <div className="aspect-[4/3] bg-secondary/40">
                      {media.mediaType === "Video" ? (
                        <video
                          src={media.url}
                          controls
                          preload="metadata"
                          className="h-full w-full object-cover"
                        />
                      ) : (
                        <img
                          src={media.url}
                          alt={media.altText || `${product.name} media ${index + 1}`}
                          className="h-full w-full object-cover"
                        />
                      )}
                    </div>
                    <div className="space-y-2 p-4">
                      <div className="flex items-center justify-between gap-3">
                        <p className="text-sm font-medium">
                          {media.altText || formatEnumLabel(media.mediaType)}
                        </p>
                        {media.isMain ? (
                          <span className="bg-primary/10 px-3 py-1 text-[11px] uppercase tracking-[0.22em] text-primary">
                            Main
                          </span>
                        ) : null}
                      </div>
                      <a
                        href={media.url}
                        target="_blank"
                        rel="noreferrer"
                        className="block break-all text-xs text-muted-foreground transition hover:text-foreground"
                      >
                        {media.url}
                      </a>
                      <p className="text-xs text-muted-foreground">
                        {media.productVariantId
                          ? `Variant: ${
                              product.variants.find((variant) => variant.id === media.productVariantId)
                                ?.name ??
                              product.variants.find((variant) => variant.id === media.productVariantId)
                                ?.sku ??
                              media.productVariantId
                            }`
                          : "Applies to the main product"}
                      </p>
                    </div>
                  </div>
                ))
              ) : (
                <div className="border border-border bg-background/80 p-6 text-sm text-muted-foreground sm:col-span-2 xl:col-span-3">
                  No media items are attached to this product.
                </div>
              )}
            </div>

            <div className="border border-border bg-background/80 p-6">
              <h2 className="text-xl font-medium tracking-tight">Catalog metadata</h2>
              <div className="mt-5 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
                <div className="border border-border bg-background/70 p-4">
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">SKU</p>
                  <p className="mt-2 text-sm">{product.sku ?? "Variant-only product"}</p>
                </div>
                <div className="border border-border bg-background/70 p-4">
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Brand</p>
                  <p className="mt-2 text-sm">{brand?.name ?? "No brand assigned"}</p>
                </div>
                <div className="border border-border bg-background/70 p-4">
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Product type</p>
                  <p className="mt-2 text-sm">{formatEnumLabel(product.productType)}</p>
                </div>
                <div className="border border-border bg-background/70 p-4">
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Lifecycle</p>
                  <p className="mt-2 text-sm">{formatEnumLabel(product.productStatus)}</p>
                </div>
                <div className="border border-border bg-background/70 p-4">
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Published</p>
                  <p className="mt-2 text-sm">{product.isPublished ? "Yes" : "No"}</p>
                </div>
                <div className="border border-border bg-background/70 p-4">
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Updated</p>
                  <p className="mt-2 text-sm">{formatDateTime(product.updatedAtUtc)}</p>
                </div>
              </div>
            </div>

            <div className="border border-border bg-background/80 p-6">
              <h2 className="text-xl font-medium tracking-tight">Categories</h2>
              <div className="mt-4 flex flex-wrap gap-2">
                {categoryIds.length ? (
                  categoryIds.map((categoryId) => (
                    <span
                      key={categoryId}
                      className="border border-border bg-secondary/60 px-4 py-2 text-sm"
                    >
                      {categoryMap.get(categoryId) ?? categoryId}
                    </span>
                  ))
                ) : (
                  <p className="text-sm text-muted-foreground">No categories assigned.</p>
                )}
              </div>
            </div>

            <div className="border border-border bg-background/80 p-6">
              <h2 className="text-xl font-medium tracking-tight">Attributes</h2>
              <div className="mt-4 space-y-3">
                {product.attributeValues.length ? (
                  product.attributeValues.map((attribute, index) => (
                    <div
                      key={`${attribute.attributeDefinitionId}-${index}`}
                      className="flex items-center justify-between gap-4 border border-border bg-background/70 px-4 py-3 text-sm"
                    >
                      <span>
                        {attributeMap.get(attribute.attributeDefinitionId) ??
                          attribute.attributeDefinitionId}
                      </span>
                      <span className="text-muted-foreground">{attribute.value}</span>
                    </div>
                  ))
                ) : (
                  <p className="text-sm text-muted-foreground">No product-level attributes set.</p>
                )}
              </div>
            </div>

            <div className="border border-border bg-background/80 p-6">
              <h2 className="text-xl font-medium tracking-tight">Variants</h2>
              <div className="mt-4 grid gap-4 md:grid-cols-2">
                {product.variants.length ? (
                  product.variants.map((variant) => (
                    <div
                      key={variant.id}
                      className="border border-border bg-background/70 p-4"
                    >
                      <div className="flex items-start justify-between gap-4">
                        <div>
                          <p className="text-sm font-medium">{variant.name ?? variant.sku}</p>
                          <p className="mt-1 text-xs text-muted-foreground">{variant.sku}</p>
                        </div>
                        <span className="bg-secondary px-3 py-1 text-[11px] uppercase tracking-[0.22em] text-muted-foreground">
                          {variant.isActive ? "Active" : "Inactive"}
                        </span>
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="text-sm text-muted-foreground">This product has no variants.</p>
                )}
              </div>
            </div>
          </div>

          <aside className="space-y-4">
            <div className="border border-border bg-gradient-to-br from-background via-background to-secondary/30 p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                Actions
              </p>
              <p className="mt-2 text-sm leading-6 text-muted-foreground">
                Manage lifecycle, open editing, or jump into downstream operational areas.
              </p>
              <div className="mt-5 space-y-3">
	                <Link
	                  href={`edit?id=${product.id}`}
	                  className="block border border-border px-4 py-3 text-center text-sm font-medium transition-colors hover:bg-secondary"
	                >
                  Edit Product
                </Link>
                <Link
                  href="/admin/inventory"
                  className="block border border-border px-4 py-3 text-center text-sm font-medium transition-colors hover:bg-secondary"
                >
                  Open Inventory
                </Link>
                <AdminProductLifecycleActions
                  productId={product.id}
                  productStatus={product.productStatus}
                  isPublished={product.isPublished}
                />
              </div>
            </div>

            <div className="border border-border bg-background/80 p-5">
              <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                Snapshot
              </p>
              <div className="mt-4 space-y-4 text-sm">
                <div>
                  <p className="text-muted-foreground">Assigned categories</p>
                  <p className="mt-1 font-medium">{categoryIds.length}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Media items</p>
                  <p className="mt-1 font-medium">{product.mediaItems.length}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Variants</p>
                  <p className="mt-1 font-medium">{product.variants.length}</p>
                </div>
              </div>
            </div>
          </aside>
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Product detail could not be loaded"
        message={getApiErrorMessage(error, "The product detail request failed.")}
      />
    )
  }
}
