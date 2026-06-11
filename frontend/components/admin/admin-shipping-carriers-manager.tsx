"use client"

import { useEffect, useState, useTransition } from "react"

import {
  createShippingCarrier,
  listShippingCarriers,
  updateShippingCarrier,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import {
  formatSubscriptionLimit,
  getSubscriptionQuotaLimit,
  subscriptionQuotaKeys,
  type TenantSubscriptionDto,
} from "@/lib/api/subscription"
import type { ShippingCarrierDto } from "@/lib/api/types"

type CarrierDraft = {
  code: string
  name: string
  serviceCode: string
  serviceName: string
  trackingUrl: string
  sortOrder: number
  isActive: boolean
}

const emptyDraft: CarrierDraft = {
  code: "",
  name: "",
  serviceCode: "",
  serviceName: "",
  trackingUrl: "",
  sortOrder: 0,
  isActive: true,
}

function toDraft(carrier: ShippingCarrierDto): CarrierDraft {
  return {
    code: carrier.code,
    name: carrier.name,
    serviceCode: carrier.serviceCode ?? "",
    serviceName: carrier.serviceName ?? "",
    trackingUrl: carrier.trackingUrl ?? "",
    sortOrder: carrier.sortOrder,
    isActive: carrier.isActive,
  }
}

export function AdminShippingCarriersManager({
  subscription,
}: {
  subscription?: TenantSubscriptionDto | null
}) {
  const [carriers, setCarriers] = useState<ShippingCarrierDto[]>([])
  const [draft, setDraft] = useState<CarrierDraft>(emptyDraft)
  const [editingCarrierId, setEditingCarrierId] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isPending, startTransition] = useTransition()
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const shippingCarrierLimit = getSubscriptionQuotaLimit(
    subscription,
    subscriptionQuotaKeys.shippingCarriers,
  )
  const activeCarrierCount = carriers.filter((carrier) => carrier.isActive).length
  const isCreateLimitReached =
    !editingCarrierId &&
    typeof shippingCarrierLimit === "number" &&
    activeCarrierCount >= shippingCarrierLimit

  async function refreshCarriers() {
    const items = await listShippingCarriers(false)
    setCarriers(items)
  }

  useEffect(() => {
    let isMounted = true

    const load = async () => {
      try {
        const items = await listShippingCarriers(false)
        if (isMounted) {
          setCarriers(items)
        }
      } catch (loadError) {
        if (isMounted) {
          setError(getApiErrorMessage(loadError, "Shipping carriers could not be loaded."))
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    void load()

    return () => {
      isMounted = false
    }
  }, [])

  function resetForm() {
    setDraft(emptyDraft)
    setEditingCarrierId(null)
  }

  function saveCarrier() {
    setError(null)
    setMessage(null)

    if (isCreateLimitReached) {
      setError(
        `Shipping carrier limit reached for your current plan (${activeCarrierCount}/${shippingCarrierLimit}).`,
      )
      return
    }

    const request = {
      code: draft.code.trim(),
      name: draft.name.trim(),
      serviceCode: draft.serviceCode.trim() || null,
      serviceName: draft.serviceName.trim() || null,
      trackingUrl: draft.trackingUrl.trim() || null,
      sortOrder: Number.isFinite(draft.sortOrder) ? draft.sortOrder : 0,
    }

    startTransition(async () => {
      try {
        if (editingCarrierId) {
          await updateShippingCarrier(editingCarrierId, {
            ...request,
            isActive: draft.isActive,
          })
          setMessage("Shipping carrier was updated.")
        } else {
          await createShippingCarrier(request)
          setMessage("Shipping carrier was added.")
        }

        await refreshCarriers()
        resetForm()
      } catch (saveError) {
        setError(getApiErrorMessage(saveError, "Shipping carrier could not be saved."))
      }
    })
  }

  function toggleCarrier(carrier: ShippingCarrierDto) {
    setError(null)
    setMessage(null)

    startTransition(async () => {
      try {
        await updateShippingCarrier(carrier.id, {
          code: carrier.code,
          name: carrier.name,
          serviceCode: carrier.serviceCode,
          serviceName: carrier.serviceName,
          trackingUrl: carrier.trackingUrl,
          sortOrder: carrier.sortOrder,
          isActive: !carrier.isActive,
        })
        await refreshCarriers()
      } catch (toggleError) {
        setError(getApiErrorMessage(toggleError, "Shipping carrier state could not be updated."))
      }
    })
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="border-b border-border pb-4 text-lg font-light">Shipping Carriers</h2>
        {typeof shippingCarrierLimit === "number" ? (
          <p className="mt-3 text-xs text-muted-foreground">
            Current plan allows {formatSubscriptionLimit(shippingCarrierLimit)} active shipping carriers.
            This store currently has {activeCarrierCount}.
          </p>
        ) : null}
      </div>

      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      ) : null}
      {message ? (
        <div className="border border-emerald-500/30 bg-emerald-500/5 px-4 py-3 text-sm text-emerald-700">
          {message}
        </div>
      ) : null}

      <div className="grid gap-4 lg:grid-cols-2">
        <div>
          <label className="mb-2 block text-sm">Carrier Code</label>
          <input
            value={draft.code}
            onChange={(event) => setDraft((current) => ({ ...current, code: event.target.value }))}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </div>
        <div>
          <label className="mb-2 block text-sm">Carrier Name</label>
          <input
            value={draft.name}
            onChange={(event) => setDraft((current) => ({ ...current, name: event.target.value }))}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </div>
        <div>
          <label className="mb-2 block text-sm">Service Code</label>
          <input
            value={draft.serviceCode}
            onChange={(event) => setDraft((current) => ({ ...current, serviceCode: event.target.value }))}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </div>
        <div>
          <label className="mb-2 block text-sm">Service Name</label>
          <input
            value={draft.serviceName}
            onChange={(event) => setDraft((current) => ({ ...current, serviceName: event.target.value }))}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </div>
        <div className="lg:col-span-2">
          <label className="mb-2 block text-sm">Tracking URL</label>
          <input
            value={draft.trackingUrl}
            onChange={(event) => setDraft((current) => ({ ...current, trackingUrl: event.target.value }))}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </div>
        <div>
          <label className="mb-2 block text-sm">Sort Order</label>
          <input
            type="number"
            value={draft.sortOrder}
            onChange={(event) =>
              setDraft((current) => ({ ...current, sortOrder: Number(event.target.value) }))
            }
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </div>
        <label className="flex items-center gap-3 pt-8 text-sm">
          <input
            type="checkbox"
            checked={draft.isActive}
            onChange={(event) => setDraft((current) => ({ ...current, isActive: event.target.checked }))}
            className="h-4 w-4"
          />
          Active
        </label>
      </div>

      <div className="flex flex-wrap justify-end gap-3 border-t border-border pt-5">
        {editingCarrierId ? (
          <button
            type="button"
            onClick={resetForm}
            className="border border-border px-6 py-3 text-sm transition-colors hover:bg-secondary"
          >
            Cancel Edit
          </button>
        ) : null}
        <button
          type="button"
          onClick={saveCarrier}
          disabled={isPending || !draft.code.trim() || !draft.name.trim() || isCreateLimitReached}
          className="bg-primary px-8 py-3 text-sm tracking-wide text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
        >
          {isPending
            ? "Saving..."
            : editingCarrierId
              ? "Update Carrier"
              : isCreateLimitReached
                ? "Carrier Limit Reached"
                : "Add Carrier"}
        </button>
      </div>

      <div className="space-y-3">
        {isLoading ? (
          <p className="text-sm text-muted-foreground">Loading shipping carriers...</p>
        ) : null}
        {!isLoading && carriers.length === 0 ? (
          <div className="border border-border px-4 py-5 text-sm text-muted-foreground">
            No shipping carriers defined yet.
          </div>
        ) : null}
        {carriers.map((carrier) => (
          <div
            key={carrier.id}
            className="grid gap-4 border border-border px-4 py-4 md:grid-cols-[1fr_auto] md:items-center"
          >
            <div>
              <div className="flex flex-wrap items-center gap-3">
                <p className="font-medium text-foreground">{carrier.name}</p>
                <span className="border border-border px-2 py-1 text-[10px] uppercase tracking-[0.2em] text-muted-foreground">
                  {carrier.code}
                </span>
                <span className="text-xs text-muted-foreground">
                  {carrier.isActive ? "Active" : "Inactive"}
                </span>
              </div>
              <p className="mt-2 text-xs text-muted-foreground">
                {carrier.serviceName || carrier.serviceCode || "Standard service"} - Order {carrier.sortOrder}
              </p>
            </div>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => {
                  setEditingCarrierId(carrier.id)
                  setDraft(toDraft(carrier))
                  setError(null)
                  setMessage(null)
                }}
                className="border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary"
              >
                Edit
              </button>
              <button
                type="button"
                onClick={() => toggleCarrier(carrier)}
                disabled={isPending}
                className="border border-border px-4 py-2 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
              >
                {carrier.isActive ? "Deactivate" : "Activate"}
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
