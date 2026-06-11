import Link from "next/link"
import { ArrowLeft, CreditCard, MapPin, Truck } from "lucide-react"
import {
  getMyOrder,
  getOrderPayment,
  getOrderShipments,
} from "@/lib/api/account"
import { getAccountPath } from "@/lib/account-path"
import { formatDate, formatDateTime, formatMoney, humanizeToken } from "@/lib/format"

export default async function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string; storeSlug?: string }>
}) {
  const { id, storeSlug } = await params
  const accountPath = getAccountPath(storeSlug)

  const [orderResult, paymentResult, shipmentsResult] = await Promise.allSettled([
    getMyOrder(id),
    getOrderPayment(id),
    getOrderShipments(id),
  ])

  if (orderResult.status !== "fulfilled") {
    throw orderResult.reason
  }

  const order = orderResult.value
  const payment = paymentResult.status === "fulfilled" ? paymentResult.value : null
  const shipments = shipmentsResult.status === "fulfilled" ? shipmentsResult.value : []

  const timeline = [
    {
      title: "Order Placed",
      date: order.placedAtUtc,
    },
    {
      title: `Payment ${humanizeToken(order.paymentStatus)}`,
      date:
        payment?.authorizedAtUtc ??
        payment?.createdAtUtc ??
        order.updatedAtUtc,
    },
    {
      title: `Fulfillment ${humanizeToken(order.fulfillmentStatus)}`,
      date: shipments[0]?.shippedAtUtc ?? order.updatedAtUtc,
    },
  ]

  return (
    <div className="space-y-10">
      <Link
        href={`${accountPath}/orders`}
        className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <ArrowLeft className="h-4 w-4" strokeWidth={1} />
        Back to Orders
      </Link>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-2xl font-serif font-light tracking-wide">
            {order.orderNumber}
          </h2>
          <p className="text-sm text-muted-foreground mt-2">
            Placed on {formatDate(order.placedAtUtc)}
          </p>
        </div>

        <div className="flex flex-wrap gap-2 text-xs tracking-[0.15em] uppercase">
          <span className="bg-secondary px-3 py-2 text-muted-foreground">
            {humanizeToken(order.status)}
          </span>
          <span className="bg-secondary px-3 py-2 text-muted-foreground">
            Payment: {humanizeToken(order.paymentStatus)}
          </span>
          <span className="bg-secondary px-3 py-2 text-muted-foreground">
            Fulfillment: {humanizeToken(order.fulfillmentStatus)}
          </span>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-8 lg:col-span-2">
          <section className="border border-border p-6">
            <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Order Timeline</h3>
            <div className="space-y-6">
              {timeline.map((step, index) => (
                <div key={`${step.title}-${index}`} className="flex gap-4">
                  <div className="mt-1 h-3 w-3 rounded-full bg-foreground" />
                  <div>
                    <p className="font-medium tracking-wide">{step.title}</p>
                    <p className="text-sm text-muted-foreground">
                      {typeof step.date === "string" ? formatDateTime(step.date) : "-"}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="border border-border p-6">
            <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Items Ordered</h3>
            <div className="space-y-4">
              {order.items.map((item) => (
                <div
                  key={item.id}
                  className="flex items-start justify-between gap-6 border-b border-border pb-4 last:border-b-0 last:pb-0"
                >
                  <div>
                    <p className="font-medium tracking-wide">{item.productName}</p>
                    <p className="text-sm text-muted-foreground mt-1">
                      {item.variantName ? `${item.variantName} - ` : ""}
                      Qty: {item.quantity}
                      {item.sku ? ` - SKU: ${item.sku}` : ""}
                    </p>
                  </div>
                  <p className="font-medium tracking-wide">
                    {formatMoney(item.lineTotalAmount, order.currencyCode)}
                  </p>
                </div>
              ))}
            </div>
          </section>

          <section className="border border-border p-6">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-xs tracking-[0.3em] uppercase">Shipments</h3>
              <Link
                href={`${accountPath}/shipments`}
                className="text-sm text-muted-foreground hover:text-foreground transition-colors"
              >
                View All Shipments
              </Link>
            </div>

            {shipments.length ? (
              <div className="space-y-4">
                {shipments.map((shipment) => (
                  <div
                    key={shipment.id}
                    className="flex flex-col gap-4 border border-border p-4 sm:flex-row sm:items-center sm:justify-between"
                  >
                    <div className="flex items-start gap-4">
                      <div className="flex h-10 w-10 items-center justify-center bg-secondary">
                        <Truck className="h-5 w-5" strokeWidth={1} />
                      </div>
                      <div>
                        <p className="font-medium tracking-wide">{shipment.shipmentNumber}</p>
                        <p className="text-sm text-muted-foreground mt-1">
                          {shipment.carrierName || "Carrier pending"}
                          {shipment.trackingNumber ? ` - ${shipment.trackingNumber}` : ""}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {humanizeToken(shipment.status)}
                        </p>
                      </div>
                    </div>

                    <Link
                      href={`${accountPath}/orders/${order.id}/shipments/${shipment.id}`}
                      className="text-sm underline underline-offset-4"
                    >
                      View Shipment
                    </Link>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">
                No shipment records are available for this order yet.
              </p>
            )}
          </section>
        </div>

        <div className="space-y-6">
          <section className="border border-border p-6">
            <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Order Summary</h3>
            <div className="space-y-4 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Subtotal</span>
                <span>{formatMoney(order.totals.subtotalAmount, order.currencyCode)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Discount</span>
                <span>{formatMoney(order.totals.discountAmount, order.currencyCode)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Shipping</span>
                <span>{formatMoney(order.totals.shippingAmount, order.currencyCode)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Tax</span>
                <span>{formatMoney(order.totals.taxAmount, order.currencyCode)}</span>
              </div>
              <div className="h-px bg-border" />
              <div className="flex justify-between font-medium text-base">
                <span>Total</span>
                <span>{formatMoney(order.totals.grandTotalAmount, order.currencyCode)}</span>
              </div>
            </div>
          </section>

          <section className="border border-border p-6">
            <div className="flex items-center gap-3 mb-6">
              <MapPin className="h-5 w-5" strokeWidth={1} />
              <h3 className="text-xs tracking-[0.3em] uppercase">Shipping Address</h3>
            </div>
            <div className="space-y-1 text-sm text-muted-foreground">
              <p className="font-medium text-foreground">{order.shippingAddress.contactName}</p>
              <p>{order.shippingAddress.line1}</p>
              {order.shippingAddress.line2 ? <p>{order.shippingAddress.line2}</p> : null}
              <p>
                {order.shippingAddress.district}, {order.shippingAddress.city}
              </p>
              <p>{order.shippingAddress.country}</p>
              {order.shippingAddress.postalCode ? <p>{order.shippingAddress.postalCode}</p> : null}
              <p>{order.shippingAddress.phoneNumber}</p>
            </div>
          </section>

          <section className="border border-border p-6">
            <div className="flex items-center gap-3 mb-6">
              <CreditCard className="h-5 w-5" strokeWidth={1} />
              <h3 className="text-xs tracking-[0.3em] uppercase">Payment</h3>
            </div>

            {payment ? (
              <div className="space-y-3 text-sm">
                <p className="font-medium tracking-wide">{humanizeToken(payment.methodType)}</p>
                <p className="text-muted-foreground">
                  Status: {humanizeToken(payment.status)}
                </p>
                <p className="text-muted-foreground">
                  Provider: {humanizeToken(payment.provider)}
                </p>
                <Link
                  href={`${accountPath}/orders/${order.id}/payment`}
                  className="inline-flex items-center gap-2 text-sm underline underline-offset-4"
                >
                  View Payment Details
                </Link>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">
                Payment details are not available right now.
              </p>
            )}
          </section>

          <div className="grid gap-3">
            <Link
              href={`${accountPath}/orders`}
              className="flex h-12 items-center justify-center border border-border text-sm tracking-wide transition-colors hover:bg-secondary/30"
            >
              All Orders
            </Link>
            <Link
              href={`${accountPath}/shipments`}
              className="flex h-12 items-center justify-center border border-border text-sm tracking-wide transition-colors hover:bg-secondary/30"
            >
              Shipment Overview
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
