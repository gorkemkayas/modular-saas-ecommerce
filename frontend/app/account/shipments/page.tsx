import Link from "next/link"
import { Package, Truck } from "lucide-react"
import { getMyOrders, getOrderShipments } from "@/lib/api/account"
import { getAccountPath } from "@/lib/account-path"
import { formatDate, humanizeToken } from "@/lib/format"

export default async function ShipmentsPage({
  params,
}: {
  params?: Promise<{ storeSlug?: string }>
}) {
  const storeSlug = (await params)?.storeSlug
  const accountPath = getAccountPath(storeSlug)
  const orders = await getMyOrders(1, 10)

  const shipmentResults = await Promise.allSettled(
    orders.items.map(async (order) => ({
      orderId: order.id,
      shipments: await getOrderShipments(order.id),
    })),
  )

  const shipments = shipmentResults
    .filter(
      (
        result,
      ): result is PromiseFulfilledResult<{
        orderId: string
        shipments: Awaited<ReturnType<typeof getOrderShipments>>
      }> => result.status === "fulfilled",
    )
    .flatMap((result) => result.value.shipments)
    .sort(
      (left, right) =>
        new Date(right.createdAtUtc).getTime() - new Date(left.createdAtUtc).getTime(),
    )

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-xs tracking-[0.3em] uppercase">Shipment Tracking</h2>
        <p className="text-sm text-muted-foreground mt-2">
          Shipment data is fetched per order because the current backend exposes shipments on order-scoped routes.
        </p>
      </div>

      {shipments.length ? (
        <div className="space-y-4">
          {shipments.map((shipment) => (
            <section key={shipment.id} className="border border-border p-6">
              <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
                <div className="flex items-start gap-4">
                  <div className="flex h-12 w-12 items-center justify-center bg-secondary">
                    <Truck className="h-5 w-5" strokeWidth={1} />
                  </div>
                  <div>
                    <div className="flex flex-wrap gap-2 mb-3 text-xs tracking-[0.15em] uppercase">
                      <span className="bg-secondary px-3 py-1 text-muted-foreground">
                        {shipment.shipmentNumber}
                      </span>
                      <span className="bg-secondary px-3 py-1 text-muted-foreground">
                        {humanizeToken(shipment.status)}
                      </span>
                    </div>
                    <p className="font-medium tracking-wide">{shipment.recipientName}</p>
                    <p className="text-sm text-muted-foreground mt-1">
                      Order {shipment.orderNumber}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {shipment.carrierName || "Carrier pending"}
                      {shipment.trackingNumber ? ` - ${shipment.trackingNumber}` : ""}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      Created on {formatDate(shipment.createdAtUtc)}
                    </p>
                  </div>
                </div>

                <div className="grid gap-3 sm:min-w-56">
                  <Link
                    href={`${accountPath}/orders/${shipment.orderId}/shipments/${shipment.id}`}
                    className="flex h-11 items-center justify-center border border-border text-sm tracking-wide transition-colors hover:bg-secondary/30"
                  >
                    View Shipment
                  </Link>
                  <Link
                    href={`${accountPath}/orders/${shipment.orderId}`}
                    className="flex h-11 items-center justify-center border border-border text-sm tracking-wide transition-colors hover:bg-secondary/30"
                  >
                    View Order
                  </Link>
                </div>
              </div>
            </section>
          ))}
        </div>
      ) : (
        <div className="border border-border p-8 text-sm text-muted-foreground">
          <div className="flex items-center gap-3">
            <Package className="h-5 w-5" strokeWidth={1} />
            No shipment records were found for the latest orders.
          </div>
        </div>
      )}
    </div>
  )
}
