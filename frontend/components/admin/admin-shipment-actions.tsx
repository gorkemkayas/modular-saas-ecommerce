"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  addShipmentPackage,
  cancelShipment,
  markShipmentDelivered,
  markShipmentReady,
  markShipmentShipped,
  registerShipmentTrackingEvent,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminShipmentActions({
  shipmentId,
  packageId,
  initialStatus,
}: {
  shipmentId: string
  packageId: string | null
  initialStatus?: string | null
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [trackingNumber, setTrackingNumber] = useState("")
  const [eventDescription, setEventDescription] = useState("Admin tracking update")
  const [cancelReason, setCancelReason] = useState("Cancelled by admin")
  const [error, setError] = useState<string | null>(null)
  const normalizedStatus = initialStatus?.trim().toLowerCase() ?? ""
  const isDelivered = normalizedStatus === "delivered"
  const isCancelled = normalizedStatus === "cancelled"
  const isShipped = normalizedStatus === "shipped"
  const isDeliveryException = normalizedStatus === "deliveryexception"
  const isTerminalShipment = isDelivered || isCancelled
  const hasPackage = Boolean(packageId)
  const canMarkReady = !isPending && !isTerminalShipment && !isShipped && !isDeliveryException && hasPackage
  const canMarkShipped = !isPending && !isTerminalShipment && hasPackage
  const canMarkDelivered = !isPending && (isShipped || isDeliveryException)
  const canCancel = !isPending && !isTerminalShipment && !isShipped && !isDeliveryException

  function run(action: () => Promise<void>, onSuccess?: () => void) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        onSuccess?.()
        router.refresh()
      } catch (actionError) {
        setError(getApiErrorMessage(actionError, "The shipment action failed."))
      }
    })
  }

  return (
    <div className="space-y-6 border border-border p-6">
      <h2 className="text-lg font-light tracking-wide">Shipment Actions</h2>

      <div className="space-y-3">
        <input
          value={trackingNumber}
          onChange={(event) => setTrackingNumber(event.target.value)}
          disabled={isPending || isTerminalShipment}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground disabled:opacity-60"
          placeholder="Tracking number, optional"
        />
        <p className="text-xs text-muted-foreground">
          Create a physical package for this shipment. Tracking number can be added now if the carrier has already provided it.
        </p>
        <button
          type="button"
          disabled={isPending || isTerminalShipment}
          onClick={() =>
            run(() =>
              addShipmentPackage(shipmentId, {
                trackingNumber: trackingNumber.trim() || null,
                weight: null,
                weightUnit: null,
                labelReference: null,
              }),
              () => setTrackingNumber(""),
            )
          }
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Create Package
        </button>
      </div>

      {packageId ? (
        <div className="space-y-3">
          <p className="text-xs text-muted-foreground">
            Add a shipment tracking update after the package moves.
            Use this for events like in transit, arrived at branch, delayed, or out for delivery.
          </p>
          <input
            placeholder="Tracking event description"
            value={eventDescription}
            onChange={(event) => setEventDescription(event.target.value)}
            disabled={isPending || isTerminalShipment}
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground disabled:opacity-60"
          />
          <button
            type="button"
            disabled={isPending || isTerminalShipment}
            onClick={() =>
              run(() =>
                registerShipmentTrackingEvent(shipmentId, {
                  packageId,
                  type: "InTransit",
                  occurredAtUtc: new Date().toISOString(),
                  location: null,
                  description: eventDescription,
                  rawStatusCode: null,
                  rawStatusText: null,
                }),
              )
            }
            className="w-full border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Add Tracking Event
          </button>
        </div>
      ) : null}

      <div className="grid gap-3 sm:grid-cols-3">
        <button
          type="button"
          disabled={!canMarkReady}
          onClick={() => run(() => markShipmentReady(shipmentId))}
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Mark Ready
        </button>
        <button
          type="button"
          disabled={!canMarkShipped}
          onClick={() => run(() => markShipmentShipped(shipmentId))}
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Mark Shipped
        </button>
        <button
          type="button"
          disabled={!canMarkDelivered}
          onClick={() => run(() => markShipmentDelivered(shipmentId))}
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Mark Delivered
        </button>
      </div>

      <div className="space-y-3">
        <input
          placeholder="Cancellation reason"
          value={cancelReason}
          onChange={(event) => setCancelReason(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
        <button
          type="button"
          disabled={!canCancel}
          onClick={() =>
            run(() => cancelShipment(shipmentId, { reason: cancelReason || null }))
          }
          className="w-full border border-destructive/30 px-4 py-3 text-sm text-destructive transition-colors hover:bg-destructive/5 disabled:opacity-60"
        >
          Cancel Shipment
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
