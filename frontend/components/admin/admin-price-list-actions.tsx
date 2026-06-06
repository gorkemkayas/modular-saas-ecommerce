"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  activatePriceList,
  archivePriceList,
  changePriceListPriority,
  deactivatePriceList,
  renamePriceList,
  setDefaultPriceList,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminPriceListActions({
  priceListId,
  initialName,
  initialPriority,
  status,
}: {
  priceListId: string
  initialName: string
  initialPriority: number
  status: string
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [name, setName] = useState(initialName)
  const [priority, setPriority] = useState(String(initialPriority))
  const [error, setError] = useState<string | null>(null)

  function run(action: () => Promise<void>) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        router.refresh()
      } catch (actionError) {
        setError(getApiErrorMessage(actionError, "The price list action failed."))
      }
    })
  }

  return (
    <div className="space-y-6 border border-border p-6">
      <h2 className="text-lg font-light tracking-wide">List Actions</h2>
      <div className="space-y-3">
        <label className="block text-sm">Name</label>
        <div className="flex gap-3">
          <input
            value={name}
            onChange={(event) => setName(event.target.value)}
            className="flex-1 bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => renamePriceList(priceListId, { name: name.trim() }))}
            className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Rename
          </button>
        </div>
      </div>
      <div className="space-y-3">
        <label className="block text-sm">Priority</label>
        <div className="flex gap-3">
          <input
            type="number"
            value={priority}
            onChange={(event) => setPriority(event.target.value)}
            className="flex-1 bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <button
            type="button"
            disabled={isPending}
            onClick={() =>
              run(() =>
                changePriceListPriority(priceListId, {
                  priority: Number(priority || "0"),
                }),
              )
            }
            className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Save
          </button>
        </div>
      </div>
      <div className="grid gap-3 sm:grid-cols-2">
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => setDefaultPriceList(priceListId))}
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Make Default
        </button>
        {status === "Active" ? (
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => deactivatePriceList(priceListId))}
            className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Deactivate
          </button>
        ) : (
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => activatePriceList(priceListId))}
            className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Activate
          </button>
        )}
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => archivePriceList(priceListId))}
          className="sm:col-span-2 border border-destructive/30 px-4 py-3 text-sm text-destructive transition-colors hover:bg-destructive/5 disabled:opacity-60"
        >
          Archive
        </button>
      </div>
      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      ) : null}
    </div>
  )
}
