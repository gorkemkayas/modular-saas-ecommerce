import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchInventoryItems } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime } from "@/lib/admin-format"

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

export default async function InventoryPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q")
  const filter = getValue(resolvedSearchParams, "filter")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchInventoryItems({
      searchTerm: query || undefined,
      onlyLowStock: filter === "low",
      pageNumber: page,
      pageSize: 15,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Inventory</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Stock management is now aligned with `api/stores/me/inventory/items`.
          </p>
        </div>

        <form className="grid gap-4 border border-border p-4 md:grid-cols-3">
          <input
            type="text"
            name="q"
            defaultValue={query}
            placeholder="Search by SKU or display name"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <select
            name="filter"
            defaultValue={filter}
            className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          >
            <option value="">All inventory</option>
            <option value="low">Low stock only</option>
          </select>
          <button className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90">
            Apply Filters
          </button>
        </form>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Item</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">On hand</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Reserved</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Available</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Threshold</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Updated</th>
                  <th className="p-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((item) => (
                  <tr key={item.id} className="hover:bg-secondary/30">
                    <td className="p-4">
                      <p className="text-sm font-medium">{item.displayName}</p>
                      <p className="text-xs text-muted-foreground">{item.sku}</p>
                    </td>
                    <td className="p-4 text-sm">{item.onHandQuantity}</td>
                    <td className="p-4 text-sm">{item.reservedQuantity}</td>
                    <td className="p-4 text-sm">{item.availableQuantity}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {item.reorderThreshold ?? "Not set"}
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(item.updatedAtUtc)}
                    </td>
                    <td className="p-4">
                      <div className="flex items-center justify-end gap-4 text-sm">
                        <Link href={`/admin/inventory/${item.id}`} className="hover:text-muted-foreground">
                          Detail
                        </Link>
                        <Link
                          href={`/admin/inventory/${item.id}/movements`}
                          className="hover:text-muted-foreground"
                        >
                          Movements
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="flex items-center justify-between gap-4">
          <p className="text-sm text-muted-foreground">
            Showing {result.items.length} of {result.totalCount} inventory items
          </p>
          <AdminPagination
            basePath="/admin/inventory"
            currentPage={result.pageNumber}
            totalPages={result.totalPages}
            query={{ q: query, filter }}
          />
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Inventory could not be loaded"
        message={getApiErrorMessage(error, "The inventory list request failed.")}
      />
    )
  }
}
