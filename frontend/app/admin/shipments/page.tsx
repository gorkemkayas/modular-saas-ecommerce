import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchStoreShipments } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

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

export default async function ShipmentsPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const status = getValue(resolvedSearchParams, "status")
  const orderNumber = getValue(resolvedSearchParams, "orderNumber")
  const trackingNumber = getValue(resolvedSearchParams, "trackingNumber")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchStoreShipments({
      status: status || undefined,
      orderNumber: orderNumber || undefined,
      trackingNumber: trackingNumber || undefined,
      pageNumber: page,
      pageSize: 15,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Shipments</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Shipment list fields now match the backend shipment summary DTO exactly.
          </p>
        </div>

        <form className="grid gap-4 border border-border p-4 md:grid-cols-4">
          <input
            type="text"
            name="orderNumber"
            defaultValue={orderNumber}
            placeholder="Order number"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <input
            type="text"
            name="trackingNumber"
            defaultValue={trackingNumber}
            placeholder="Tracking number"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <select
            name="status"
            defaultValue={status}
            className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          >
            <option value="">All statuses</option>
            <option value="Pending">Pending</option>
            <option value="ReadyForDispatch">Ready for dispatch</option>
            <option value="Shipped">Shipped</option>
            <option value="Delivered">Delivered</option>
            <option value="Cancelled">Cancelled</option>
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
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Shipment</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Recipient</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Carrier</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Tracking</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Created</th>
                  <th className="p-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Detail</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((shipment) => (
                  <tr key={shipment.id} className="hover:bg-secondary/30">
                    <td className="p-4">
                      <p className="text-sm font-medium">{shipment.shipmentNumber}</p>
                      <p className="text-xs text-muted-foreground">{shipment.orderNumber}</p>
                    </td>
                    <td className="p-4 text-sm">{shipment.recipientName}</td>
                    <td className="p-4 text-sm">{shipment.carrierName ?? "Not assigned"}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {shipment.trackingNumber ?? "Not assigned"}
                    </td>
                    <td className="p-4 text-sm">{formatEnumLabel(shipment.status)}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(shipment.createdAtUtc)}
                    </td>
                    <td className="p-4 text-right">
                      <Link href={`/admin/shipments/${shipment.id}`} className="text-sm hover:text-muted-foreground">
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <AdminPagination
          basePath="/admin/shipments"
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{ status, orderNumber, trackingNumber }}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Shipments could not be loaded"
        message={getApiErrorMessage(error, "The shipment search request failed.")}
      />
    )
  }
}
