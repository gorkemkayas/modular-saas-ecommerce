import Link from "next/link"
import { ArrowLeft, Calendar, MapPin, Package, Truck } from "lucide-react"
import { getOrderShipmentById } from "@/lib/api/account"
import { getAccountPath } from "@/lib/account-path"
import { formatDateTime, humanizeToken } from "@/lib/format"
import { buildCarrierTrackingUrl } from "@/lib/shipment-tracking"

export default async function ShipmentDetailPage({
  params,
}: {
  params: Promise<{ id: string; shipmentId: string; storeSlug?: string }>
}) {
  const { id, shipmentId, storeSlug } = await params
  const accountPath = getAccountPath(storeSlug)
  const shipment = await getOrderShipmentById(id, shipmentId)
  const firstTrackingNumber =
    shipment.packages.find((shipmentPackage) => shipmentPackage.trackingNumber?.trim())
      ?.trackingNumber ?? null
  const carrierTrackingUrl = buildCarrierTrackingUrl(
    shipment.trackingUrl,
    firstTrackingNumber,
  )

  const trackingEvents = shipment.packages
    .flatMap((shipmentPackage) =>
      shipmentPackage.trackingEvents.map((event) => ({
        ...event,
        packageNumber: shipmentPackage.packageNumber,
        trackingNumber: shipmentPackage.trackingNumber,
      })),
    )
    .sort(
      (left, right) =>
        new Date(right.occurredAtUtc).getTime() - new Date(left.occurredAtUtc).getTime(),
    )

  return (
    <div className="space-y-8">
      <Link
        href={`${accountPath}/orders/${id}`}
        className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <ArrowLeft className="h-4 w-4" strokeWidth={1} />
        Back to Order
      </Link>

      <div>
        <h2 className="text-xs tracking-[0.3em] uppercase">Shipment Details</h2>
        <p className="text-sm text-muted-foreground mt-2">
          Tracking information for shipment {shipment.shipmentNumber}.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <section className="border border-border p-6">
          <div className="flex items-center gap-3 mb-4">
            <Truck className="h-5 w-5" strokeWidth={1} />
            <h3 className="text-xs tracking-[0.2em] uppercase">Carrier</h3>
          </div>
          <p className="font-medium tracking-wide">
            {shipment.carrierName || "Carrier pending"}
          </p>
          <p className="text-sm text-muted-foreground mt-2">
            Status: {humanizeToken(shipment.status)}
          </p>
          {carrierTrackingUrl ? (
            <a
              href={carrierTrackingUrl}
              target="_blank"
              rel="noreferrer"
              className="inline-block mt-4 text-sm underline underline-offset-4"
            >
              Open carrier tracking page
            </a>
          ) : null}
        </section>

        <section className="border border-border p-6">
          <div className="flex items-center gap-3 mb-4">
            <MapPin className="h-5 w-5" strokeWidth={1} />
            <h3 className="text-xs tracking-[0.2em] uppercase">Destination</h3>
          </div>
          <div className="space-y-1 text-sm text-muted-foreground">
            <p className="font-medium text-foreground">{shipment.destinationAddress.contactName}</p>
            <p>{shipment.destinationAddress.line1}</p>
            {shipment.destinationAddress.line2 ? <p>{shipment.destinationAddress.line2}</p> : null}
            <p>
              {shipment.destinationAddress.district}, {shipment.destinationAddress.city}
            </p>
            <p>{shipment.destinationAddress.country}</p>
            {shipment.destinationAddress.postalCode ? (
              <p>{shipment.destinationAddress.postalCode}</p>
            ) : null}
          </div>
        </section>
      </div>

      <section className="border border-border p-6">
        <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Tracking Timeline</h3>
        {trackingEvents.length ? (
          <div className="space-y-6">
            {trackingEvents.map((event, index) => {
              const eventTrackingUrl = buildCarrierTrackingUrl(
                shipment.trackingUrl,
                event.trackingNumber ?? firstTrackingNumber,
              )

              return (
                <div key={event.id} className="flex gap-4">
                  <div className="mt-1 h-3 w-3 rounded-full bg-foreground" />
                  <div>
                    <p className="font-medium tracking-wide">{event.description}</p>
                    <div className="mt-1 flex flex-wrap gap-4 text-sm text-muted-foreground">
                      <span className="inline-flex items-center gap-2">
                        <Calendar className="h-4 w-4" strokeWidth={1} />
                        {formatDateTime(event.occurredAtUtc)}
                      </span>
                      {event.location ? (
                        <span className="inline-flex items-center gap-2">
                          <MapPin className="h-4 w-4" strokeWidth={1} />
                          {event.location}
                        </span>
                      ) : null}
                      <span>Package {event.packageNumber}</span>
                    </div>
                    {index === 0 && eventTrackingUrl ? (
                      <a
                        href={eventTrackingUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="mt-2 inline-block text-sm underline underline-offset-4"
                      >
                        Open tracking page
                      </a>
                    ) : null}
                  </div>
                </div>
              )
            })}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">
            No tracking events have been recorded yet.
          </p>
        )}
      </section>

      <section className="border border-border p-6">
        <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Shipment Contents</h3>
        <div className="space-y-4">
          {shipment.lines.map((line) => (
            <div key={line.id} className="flex items-center justify-between border border-border p-4">
              <div className="flex items-start gap-3">
                <Package className="mt-0.5 h-4 w-4" strokeWidth={1} />
                <div>
                  <p className="font-medium tracking-wide">{line.productName}</p>
                  <p className="text-sm text-muted-foreground mt-1">
                    {line.variantName ? `${line.variantName} - ` : ""}
                    Qty: {line.quantity}
                    {line.sku ? ` - SKU: ${line.sku}` : ""}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
