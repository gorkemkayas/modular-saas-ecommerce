"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import { activateCustomer, blockCustomer } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminCustomerActions({
  customerId,
  status,
}: {
  customerId: string
  status: string
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [error, setError] = useState<string | null>(null)

  function run(action: () => Promise<void>) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        router.refresh()
      } catch (actionError) {
        setError(getApiErrorMessage(actionError, "The customer action failed."))
      }
    })
  }

  return (
    <div className="space-y-3 border border-border p-6">
      <h2 className="text-lg font-light tracking-wide">Customer Actions</h2>
      {status === "Blocked" ? (
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => activateCustomer(customerId))}
          className="w-full border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Activate Customer
        </button>
      ) : (
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => blockCustomer(customerId))}
          className="w-full border border-destructive/30 px-4 py-3 text-sm text-destructive transition-colors hover:bg-destructive/5 disabled:opacity-60"
        >
          Block Customer
        </button>
      )}
      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      ) : null}
    </div>
  )
}
