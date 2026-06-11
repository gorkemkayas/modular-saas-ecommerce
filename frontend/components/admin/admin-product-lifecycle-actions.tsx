"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  activateProduct,
  archiveProduct,
  publishProduct,
  unpublishProduct,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminProductLifecycleActions({
  productId,
  productStatus,
  isPublished,
}: {
  productId: string
  productStatus: string
  isPublished: boolean
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
        setError(getApiErrorMessage(actionError, "The product action failed."))
      }
    })
  }

  return (
    <div className="space-y-3">
      {productStatus === "Draft" ? (
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => activateProduct(productId))}
          className="block w-full border border-border px-4 py-3 text-center text-sm font-medium transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Activate Product
        </button>
      ) : null}
      {!isPublished ? (
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => publishProduct(productId))}
          className="block w-full border border-border px-4 py-3 text-center text-sm font-medium transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Publish Product
        </button>
      ) : (
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => unpublishProduct(productId))}
          className="block w-full border border-border px-4 py-3 text-center text-sm font-medium transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Unpublish Product
        </button>
      )}
      {productStatus !== "Archived" ? (
        <button
          type="button"
          disabled={isPending}
          onClick={() => run(() => archiveProduct(productId))}
          className="block w-full border border-destructive/30 px-4 py-3 text-center text-sm font-medium text-destructive transition-colors hover:bg-destructive/5 disabled:opacity-60"
        >
          Archive Product
        </button>
      ) : null}
      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      ) : null}
    </div>
  )
}
