import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminShipmentActions } from "@/components/admin/admin-shipment-actions"
import { getStoreShipmentById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

export default async function AdminShipmentDetailPage({ params }: { params: { id: string } }) {
  try {
    const shipment = await getStoreShipmentById(params.id)
    const trackingEvents = shipment.packages.flatMap((packageItem) =>
      packageItem.trackingEvents.map((event) => ({
        ...event,
        packageNumber: packageItem.packageNumber,
      })),
    )

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/shipments" className="hover:text-foreground">
            Shipments
          </Link>
          <span>/</span>
          <span className="text-foreground">{shipment.shipmentNumber}</span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">{shipment.shipmentNumber}</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            {formatEnumLabel(shipment.status)} • order {shipment.orderNumber}
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Recipient</p>
            <p className="mt-2 text-sm">{shipment.recipientName}</p>
            <p className="text-sm text-muted-foreground">{shipment.recipientPhoneNumber}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Carrier</p>
            <p className="mt-2 text-sm">{shipment.carrierName ?? "Not assigned"}</p>
            <p className="text-sm text-muted-foreground">{shipment.serviceName ?? "No service selected"}</p>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Destination</h2>
          <div className="mt-4 text-sm text-muted-foreground">
            <p>{shipment.destinationAddress.line1}</p>
            {shipment.destinationAddress.line2 ? <p>{shipment.destinationAddress.line2}</p> : null}
            <p>
              {shipment.destinationAddress.district}, {shipment.destinationAddress.city}
            </p>
            <p>
              {shipment.destinationAddress.country} {shipment.destinationAddress.postalCode ?? ""}
            </p>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Packages</h2>
          <div className="mt-4 space-y-4">
            {shipment.packages.length ? (
              shipment.packages.map((packageItem) => (
                <div key={packageItem.id} className="border border-border p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-sm font-medium">{packageItem.packageNumber}</p>
                      <p className="text-xs text-muted-foreground">
                        {packageItem.trackingNumber ?? "No tracking number"}
                      </p>
                    </div>
                    <span className="text-xs text-muted-foreground">
                      {packageItem.weight ? `${packageItem.weight} ${packageItem.weightUnit ?? ""}` : "No weight"}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No packages have been added.</p>
            )}
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Tracking Events</h2>
          <div className="mt-4 space-y-4">
            {trackingEvents.length ? (
              trackingEvents.map((event) => (
                <div key={event.id} className="border-l border-border pl-4">
                  <p className="text-sm font-medium">
                    {formatEnumLabel(event.type)} • {event.packageNumber}
                  </p>
                  <p className="text-xs text-muted-foreground">{formatDateTime(event.occurredAtUtc)}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{event.description}</p>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No tracking events recorded yet.</p>
            )}
          </div>
        </div>

        <AdminShipmentActions
          shipmentId={shipment.id}
          packageId={shipment.packages[0]?.id ?? null}
          initialStatus={shipment.status}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Shipment detail could not be loaded"
        message={getApiErrorMessage(error, "The shipment detail request failed.")}
      />
    )
  }
}
