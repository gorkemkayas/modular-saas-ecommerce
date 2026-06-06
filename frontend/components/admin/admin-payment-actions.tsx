"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  cancelStorePayment,
  captureStorePayment,
  refundStorePayment,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminPaymentActions({
  paymentId,
  amount,
}: {
  paymentId: string
  amount: number
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [refundAmount, setRefundAmount] = useState(String(amount))
  const [refundReason, setRefundReason] = useState("Admin refund")
  const [error, setError] = useState<string | null>(null)

  function run(action: () => Promise<void>) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        router.refresh()
      } catch (actionError) {
        setError(getApiErrorMessage(actionError, "The payment action failed."))
      }
    })
  }

  return (
    <div className="space-y-4 border border-border p-6">
      <h2 className="text-lg font-light tracking-wide">Payment Actions</h2>
      <div className="grid gap-3 sm:grid-cols-2">
        <button
          type="button"
          disabled={isPending}
          onClick={() =>
            run(() =>
              captureStorePayment(paymentId, {
                idempotencyKey: crypto.randomUUID(),
              }),
            )
          }
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Capture
        </button>
        <button
          type="button"
          disabled={isPending}
          onClick={() =>
            run(() =>
              cancelStorePayment(paymentId, {
                idempotencyKey: crypto.randomUUID(),
              }),
            )
          }
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Cancel
        </button>
      </div>
      <div className="space-y-3">
        <label className="block text-sm">Refund Amount</label>
        <input
          type="number"
          step="0.01"
          value={refundAmount}
          onChange={(event) => setRefundAmount(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
        <label className="block text-sm">Refund Reason</label>
        <input
          value={refundReason}
          onChange={(event) => setRefundReason(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
        <button
          type="button"
          disabled={isPending}
          onClick={() =>
            run(() =>
              refundStorePayment(paymentId, {
                amount: Number(refundAmount || "0"),
                reason: refundReason,
                idempotencyKey: crypto.randomUUID(),
              }),
            )
          }
          className="w-full border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Refund
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
