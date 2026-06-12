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

export default async function StockMovementsPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchInventoryItems({
      searchTerm: query || undefined,
      pageNumber: page,
      pageSize: 15,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Stock Movements</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            The backend exposes stock history per inventory item, so this page links into each item&apos;s movement ledger instead of inventing a global feed.
          </p>
        </div>

        <form className="max-w-md">
          <input
            type="text"
            name="q"
            defaultValue={query}
            placeholder="Find an item by SKU or display name"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </form>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Item</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">SKU</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Availability</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Last change</th>
                  <th className="p-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Ledger</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((item) => (
                  <tr key={item.id} className="hover:bg-secondary/30">
                    <td className="p-4 text-sm font-medium">{item.displayName}</td>
                    <td className="p-4 text-sm text-muted-foreground">{item.sku}</td>
                    <td className="p-4 text-sm">
                      {item.availableQuantity} available / {item.onHandQuantity} on hand
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(item.updatedAtUtc)}
                    </td>
                    <td className="p-4 text-right">
                      <Link
                        href={`/admin/inventory/${item.id}/movements`}
                        className="text-sm transition-colors hover:text-muted-foreground"
                      >
                        View movements
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <AdminPagination
          basePath="/admin/stock-movements"
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{ q: query }}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Stock movement explorer could not be loaded"
        message={getApiErrorMessage(error, "The inventory lookup request failed.")}
      />
    )
  }
}
