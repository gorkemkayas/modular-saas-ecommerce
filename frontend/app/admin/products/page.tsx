import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchProducts } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"
import { withQuery } from "@/lib/config"

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>
}

function getValue(
  searchParams: Record<string, string | string[] | undefined>,
  key: string,
): string {
  const value = searchParams[key]
  return typeof value === "string" ? value : ""
}

function getPage(searchParams: Record<string, string | string[] | undefined>): number {
  const rawValue = getValue(searchParams, "page")
  const parsedValue = Number.parseInt(rawValue, 10)
  return Number.isFinite(parsedValue) && parsedValue > 0 ? parsedValue : 1
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

export default async function AdminProductsPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q")
  const status = getValue(resolvedSearchParams, "status")
  const published = getValue(resolvedSearchParams, "published")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchProducts({
      searchTerm: query || undefined,
      status: status || undefined,
      isPublished:
        published === "true" ? true : published === "false" ? false : undefined,
      pageNumber: page,
      pageSize: 12,
    })

    const publishedCount = result.items.filter((product) => product.isPublished).length
    const draftCount = result.items.filter(
      (product) => product.productStatus === "Draft",
    ).length
    const variantCount = result.items.filter(
      (product) => product.productType === "Variant",
    ).length
    const simpleCount = result.items.length - variantCount

    return (
      <div className="space-y-6">
        <div className="border border-border bg-gradient-to-br from-background via-background to-secondary/40 p-6">
          <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <p className="text-[11px] uppercase tracking-[0.32em] text-muted-foreground">
                Catalog overview
              </p>
              <h1 className="mt-2 text-3xl font-medium tracking-tight">Products</h1>
              <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground">
                Review the backend product catalog, scan lifecycle status quickly, and
                jump into richer detail or editing flows without losing context.
              </p>
            </div>
            <Link
              href="/admin/products/create"
              className="inline-flex items-center justify-center bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
            >
              New Product
            </Link>
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <div className="border border-border bg-background/80 p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
              Results
            </p>
            <p className="mt-3 text-3xl font-medium tracking-tight">{result.items.length}</p>
            <p className="mt-2 text-sm text-muted-foreground">
              Visible products on this page
            </p>
          </div>
          <div className="border border-border bg-background/80 p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
              Published
            </p>
            <p className="mt-3 text-3xl font-medium tracking-tight">{publishedCount}</p>
            <p className="mt-2 text-sm text-muted-foreground">Live in storefront-ready state</p>
          </div>
          <div className="border border-border bg-background/80 p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
              Drafts
            </p>
            <p className="mt-3 text-3xl font-medium tracking-tight">{draftCount}</p>
            <p className="mt-2 text-sm text-muted-foreground">Still waiting for activation</p>
          </div>
          <div className="border border-border bg-background/80 p-5">
            <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
              Mix
            </p>
            <p className="mt-3 text-lg font-medium tracking-tight">
              {simpleCount} simple / {variantCount} variant
            </p>
            <p className="mt-2 text-sm text-muted-foreground">Current product type balance</p>
          </div>
        </div>

        <form className="border border-border bg-background/80 p-5">
          <div className="mb-4 flex flex-col gap-1">
            <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
              Filters
            </p>
            <p className="text-sm text-muted-foreground">
              Narrow the backend catalog response by search term, lifecycle, or publish state.
            </p>
          </div>
          <div className="grid gap-4 md:grid-cols-4">
            <input
              type="text"
              name="q"
              defaultValue={query}
              placeholder="Search by name or slug"
              className="w-full border border-border bg-background px-4 py-3 text-sm focus:border-foreground/30 focus:outline-none focus:ring-1 focus:ring-foreground"
            />
            <select
              name="status"
              defaultValue={status}
              className="border border-border bg-background px-4 py-3 text-sm focus:border-foreground/30 focus:outline-none focus:ring-1 focus:ring-foreground"
            >
              <option value="">All lifecycle states</option>
              <option value="Draft">Draft</option>
              <option value="Active">Active</option>
              <option value="Archived">Archived</option>
            </select>
            <select
              name="published"
              defaultValue={published}
              className="border border-border bg-background px-4 py-3 text-sm focus:border-foreground/30 focus:outline-none focus:ring-1 focus:ring-foreground"
            >
              <option value="">All publication states</option>
              <option value="true">Published</option>
              <option value="false">Unpublished</option>
            </select>
            <button className="bg-primary px-4 py-3 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90">
              Apply Filters
            </button>
          </div>
        </form>

        <div className="overflow-hidden border border-border bg-background/80">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/30">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Name</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Slug</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Type</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Lifecycle</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Published</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Updated</th>
                  <th className="p-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Detail</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((product) => (
                  <tr key={product.id} className="transition hover:bg-secondary/20">
                    <td className="p-4">
                      <div>
                        <p className="text-sm font-medium">{product.name}</p>
                        <p className="mt-1 text-xs text-muted-foreground">{product.id}</p>
                      </div>
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">/{product.slug}</td>
                    <td className="p-4 text-sm">
                      <span className="bg-secondary px-3 py-1 text-xs font-medium text-foreground">
                        {formatEnumLabel(product.productType)}
                      </span>
                    </td>
                    <td className="p-4 text-sm">
                      <span
                        className={`px-3 py-1 text-xs font-medium ${getLifecycleClasses(
                          product.productStatus,
                        )}`}
                      >
                        {formatEnumLabel(product.productStatus)}
                      </span>
                    </td>
                    <td className="p-4 text-sm">
                      <span
                        className={`px-3 py-1 text-xs font-medium ${
                          product.isPublished
                            ? "bg-emerald-500/10 text-emerald-700 dark:text-emerald-300"
                            : "bg-muted text-muted-foreground"
                        }`}
                      >
                        {product.isPublished ? "Published" : "Private"}
                      </span>
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(product.updatedAtUtc)}
                    </td>
                    <td className="p-4 text-right">
                      <Link
                        href={`/admin/products/${product.id}`}
                        className="inline-flex border border-border px-4 py-2 text-sm font-medium transition-colors hover:bg-secondary"
                      >
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {!result.items.length ? (
          <div className="border border-border bg-background/80 p-6 text-sm text-muted-foreground">
            No products matched the current backend filters.
          </div>
        ) : null}

        <div className="flex items-center justify-between gap-4">
          <p className="text-sm text-muted-foreground">
            Showing {result.items.length} of {result.totalCount} products
          </p>
          <AdminPagination
            basePath="/admin/products"
            currentPage={result.pageNumber}
            totalPages={result.totalPages}
            query={{ q: query, status, published }}
          />
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Products could not be loaded"
        message={getApiErrorMessage(error, "The product list request failed.")}
      />
    )
  }
}
