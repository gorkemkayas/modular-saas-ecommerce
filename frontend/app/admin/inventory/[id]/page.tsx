import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminInventoryActions } from "@/components/admin/admin-inventory-actions"
import { getInventoryItemById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

export default async function AdminInventoryDetailPage({ params }: { params: { id: string } }) {
  try {
    const item = await getInventoryItemById(params.id)

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/inventory" className="hover:text-foreground">
            Inventory
          </Link>
          <span>/</span>
          <span className="text-foreground">{item.displayName}</span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">{item.displayName}</h1>
          <p className="mt-2 text-sm text-muted-foreground">{item.sku}</p>
        </div>

        <div className="grid gap-4 md:grid-cols-4">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">On hand</p>
            <p className="mt-2 text-2xl font-light">{item.onHandQuantity}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Reserved</p>
            <p className="mt-2 text-2xl font-light">{item.reservedQuantity}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Available</p>
            <p className="mt-2 text-2xl font-light">{item.availableQuantity}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Threshold</p>
            <p className="mt-2 text-2xl font-light">{item.reorderThreshold ?? "-"}</p>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Reservations</h2>
          <div className="mt-4 space-y-3">
            {item.reservations.length ? (
              item.reservations.map((reservation) => (
                <div key={reservation.id} className="flex items-center justify-between gap-4 border-b border-border pb-3 text-sm last:border-b-0 last:pb-0">
                  <div>
                    <p>{reservation.reservationReference}</p>
                    <p className="text-xs text-muted-foreground">{reservation.orderId}</p>
                  </div>
                  <div className="text-right">
                    <p>{reservation.quantity}</p>
                    <p className="text-xs text-muted-foreground">{formatEnumLabel(reservation.status)}</p>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No active or historical reservations.</p>
            )}
          </div>
        </div>

        <div className="border border-border p-6">
          <div className="flex items-center justify-between gap-4">
            <h2 className="text-lg font-light tracking-wide">Recent Movements</h2>
            <Link href={`/admin/inventory/${item.id}/movements`} className="text-sm hover:text-muted-foreground">
              Full Ledger
            </Link>
          </div>
          <div className="mt-4 space-y-3">
            {item.recentMovements.length ? (
              item.recentMovements.map((movement) => (
                <div key={movement.id} className="border border-border p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-sm font-medium">{formatEnumLabel(movement.type)}</p>
                      <p className="text-xs text-muted-foreground">{movement.reason}</p>
                    </div>
                    <span className="text-xs text-muted-foreground">
                      {formatDateTime(movement.createdAtUtc)}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No recent movements.</p>
            )}
          </div>
        </div>

        <AdminInventoryActions
          inventoryItemId={item.id}
          onHandQuantity={item.onHandQuantity}
          reorderThreshold={item.reorderThreshold}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Inventory detail could not be loaded"
        message={getApiErrorMessage(error, "The inventory detail request failed.")}
      />
    )
  }
}
