"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  addStockToInventoryItem,
  adjustInventoryItemStock,
  setInventoryReorderThreshold,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminInventoryActions({
  inventoryItemId,
  onHandQuantity,
  reorderThreshold,
}: {
  inventoryItemId: string
  onHandQuantity: number
  reorderThreshold: number | null
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [addQuantity, setAddQuantity] = useState("0")
  const [adjustQuantity, setAdjustQuantity] = useState(String(onHandQuantity))
  const [threshold, setThreshold] = useState(reorderThreshold?.toString() ?? "")
  const [reason, setReason] = useState("Manual admin adjustment")
  const [error, setError] = useState<string | null>(null)

  function run(action: () => Promise<void>) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        router.refresh()
      } catch (actionError) {
        setError(getApiErrorMessage(actionError, "The inventory action failed."))
      }
    })
  }

  return (
    <div className="space-y-6 border border-border p-6">
      <div>
        <h2 className="text-lg font-light tracking-wide">Inventory Actions</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          These controls write directly to the inventory command endpoints.
        </p>
      </div>

      <div className="space-y-3">
        <label className="block text-sm">Reason</label>
        <input
          value={reason}
          onChange={(event) => setReason(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div className="space-y-3">
          <label className="block text-sm">Add Stock Quantity</label>
          <input
            type="number"
            value={addQuantity}
            onChange={(event) => setAddQuantity(event.target.value)}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <button
            type="button"
            disabled={isPending}
            onClick={() =>
              run(() =>
                addStockToInventoryItem(inventoryItemId, {
                  quantity: Number(addQuantity || "0"),
                  reason,
                  reference: null,
                }),
              )
            }
            className="w-full border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Add Stock
          </button>
        </div>

        <div className="space-y-3">
          <label className="block text-sm">Set On Hand Quantity</label>
          <input
            type="number"
            value={adjustQuantity}
            onChange={(event) => setAdjustQuantity(event.target.value)}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <button
            type="button"
            disabled={isPending}
            onClick={() =>
              run(() =>
                adjustInventoryItemStock(inventoryItemId, {
                  newOnHandQuantity: Number(adjustQuantity || "0"),
                  reason,
                  reference: null,
                }),
              )
            }
            className="w-full border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Adjust Stock
          </button>
        </div>

        <div className="space-y-3">
          <label className="block text-sm">Reorder Threshold</label>
          <input
            type="number"
            value={threshold}
            onChange={(event) => setThreshold(event.target.value)}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <button
            type="button"
            disabled={isPending}
            onClick={() =>
              run(() =>
                setInventoryReorderThreshold(inventoryItemId, {
                  reorderThreshold: threshold.trim() ? Number(threshold) : null,
                }),
              )
            }
            className="w-full border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Save Threshold
          </button>
        </div>
      </div>

      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      ) : null}
    </div>
  )
}
