"use client"

import { use, useEffect, useState } from "react"
import Link from "next/link"
import { useSearchParams } from "next/navigation"
import { CheckCircle, Copy } from "lucide-react"
import { Button } from "@/components/ui/button"
import { getMyOrder } from "@/lib/api/account"
import { getAccountPath } from "@/lib/account-path"
import type { OrderDto } from "@/lib/api/types"
import { formatMoney } from "@/lib/format"
import { useStore } from "@/lib/store-context"

export function StoreOrderSuccessContent({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = use(params)
  const ordersHref = getAccountPath(storeSlug, "/orders")
  const searchParams = useSearchParams()
  const orderId = searchParams.get("orderId")
  const { clearCart } = useStore()
  const [order, setOrder] = useState<OrderDto | null>(null)

  useEffect(() => {
    clearCart()
  }, [clearCart])

  useEffect(() => {
    if (!orderId) {
      return
    }

    void getMyOrder(orderId)
      .then(setOrder)
      .catch(() => setOrder(null))
  }, [orderId])

  function copyOrderId() {
    if (order?.orderNumber) {
      void navigator.clipboard.writeText(order.orderNumber)
    }
  }

  return (
    <div className="min-h-screen bg-background">
      <section className="border-b border-border">
        <div className="mx-auto max-w-3xl px-6 py-16 text-center lg:py-24">
          <div className="mx-auto mb-8 flex h-16 w-16 items-center justify-center bg-foreground">
            <CheckCircle className="h-8 w-8 text-background" strokeWidth={1} />
          </div>

          <h1 className="mb-4 font-serif text-3xl font-light tracking-wide lg:text-5xl">
            Thank You for Your Order
          </h1>
          <p className="mx-auto max-w-md text-muted-foreground">
            Your order has been created in the commerce backend and is now available under your account.
          </p>

          {order ? (
            <div className="mt-8 inline-flex items-center gap-3 bg-secondary px-6 py-3">
              <span className="text-sm text-muted-foreground">Order Number:</span>
              <span className="font-mono font-medium">{order.orderNumber}</span>
              <button
                onClick={copyOrderId}
                className="text-muted-foreground transition-colors hover:text-foreground"
              >
                <Copy className="h-4 w-4" strokeWidth={1} />
              </button>
            </div>
          ) : null}
        </div>
      </section>

      <div className="mx-auto max-w-5xl px-6 py-12">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="space-y-8 lg:col-span-2">
            <section className="border border-border p-6">
              <h2 className="mb-6 text-xs uppercase tracking-[0.3em]">Order Status</h2>
              {order ? (
                <div className="space-y-3 text-sm">
                  <p>Status: {order.status}</p>
                  <p>Payment: {order.paymentStatus}</p>
                  <p>Fulfillment: {order.fulfillmentStatus}</p>
                  <p>Placed At: {new Date(order.placedAtUtc).toLocaleString("tr-TR")}</p>
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">
                  We couldn't fetch full order details yet. You can still open the order from your account.
                </p>
              )}
            </section>

            {order ? (
              <section className="border border-border p-6">
                <h2 className="mb-6 text-xs uppercase tracking-[0.3em]">Items Ordered</h2>
                <div className="space-y-4">
                  {order.items.map((item) => (
                    <div
                      key={item.id}
                      className="flex items-center justify-between border-b border-border pb-4 last:border-b-0"
                    >
                      <div>
                        <p className="font-medium">{item.productName}</p>
                        <p className="text-sm text-muted-foreground">
                          {item.variantName || "Base product"} - Qty {item.quantity}
                        </p>
                      </div>
                      <p className="text-sm">
                        {formatMoney(item.lineTotalAmount, order.currencyCode)}
                      </p>
                    </div>
                  ))}
                </div>
              </section>
            ) : null}
          </div>

          <div className="space-y-6">
            {order ? (
              <section className="border border-border p-6">
                <h2 className="mb-6 text-xs uppercase tracking-[0.3em]">Order Summary</h2>
                <div className="space-y-4 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Subtotal</span>
                    <span>{formatMoney(order.totals.subtotalAmount, order.currencyCode)}</span>
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
                  <div className="flex justify-between text-base font-medium">
                    <span>Total</span>
                    <span>{formatMoney(order.totals.grandTotalAmount, order.currencyCode)}</span>
                  </div>
                </div>
              </section>
            ) : null}

            <div className="space-y-3">
              <Link href={ordersHref} className="block">
                <Button className="h-12 w-full bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase">
                  Open My Orders
                </Button>
              </Link>
              <Link href={`/${storeSlug}/products`} className="block">
                <Button
                  variant="outline"
                  className="h-12 w-full border-border text-sm tracking-wide"
                >
                  Continue Shopping
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
