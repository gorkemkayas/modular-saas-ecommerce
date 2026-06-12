import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { getInventoryItemById, getInventoryMovements } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

type Props = {
  params: { id: string }
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

export default async function AdminInventoryMovementsPage({ params, searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const page = getPage(resolvedSearchParams)

  try {
    const [item, result] = await Promise.all([
      getInventoryItemById(params.id),
      getInventoryMovements(params.id, page, 20),
    ])

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/inventory" className="hover:text-foreground">
            Inventory
          </Link>
          <span>/</span>
          <Link href={`/admin/inventory/${params.id}`} className="hover:text-foreground">
            {item.displayName}
          </Link>
          <span>/</span>
          <span className="text-foreground">Movements</span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">Stock Movements</h1>
          <p className="mt-2 text-sm text-muted-foreground">{item.displayName} • {item.sku}</p>
        </div>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Type</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">On hand delta</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Reserved delta</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Result</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Reason</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Created</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((movement) => (
                  <tr key={movement.id} className="hover:bg-secondary/30">
                    <td className="px-6 py-4 text-sm">{formatEnumLabel(movement.type)}</td>
                    <td className="px-6 py-4 text-sm">{movement.onHandDelta}</td>
                    <td className="px-6 py-4 text-sm">{movement.reservedDelta}</td>
                    <td className="px-6 py-4 text-sm">
                      {movement.resultingOnHandQuantity} / {movement.resultingReservedQuantity}
                    </td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">
                      {movement.reference ? `${movement.reason} • ${movement.reference}` : movement.reason}
                    </td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">
                      {formatDateTime(movement.createdAtUtc)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <AdminPagination
          basePath={`/admin/inventory/${params.id}/movements`}
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{}}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Inventory movements could not be loaded"
        message={getApiErrorMessage(error, "The inventory movement request failed.")}
      />
    )
  }
}
