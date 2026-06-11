import Link from "next/link"
import { ChevronLeft, ChevronRight, Package } from "lucide-react"
import { getMyOrders } from "@/lib/api/account"
import { getAccountPath } from "@/lib/account-path"
import { formatDate, formatMoney, humanizeToken } from "@/lib/format"

export default async function OrdersPage({
  params,
  searchParams,
}: {
  params?: Promise<{ storeSlug?: string }>
  searchParams: Promise<{ page?: string }>
}) {
  const storeSlug = (await params)?.storeSlug
  const accountPath = getAccountPath(storeSlug)
  const resolvedSearchParams = await searchParams
  const pageNumber = Number(resolvedSearchParams.page ?? "1")
  const orders = await getMyOrders(Number.isNaN(pageNumber) ? 1 : pageNumber, 12)

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xs tracking-[0.3em] uppercase">Order History</h2>
          <p className="text-sm text-muted-foreground mt-2">
            Review order, payment, and fulfillment status from your backend customer account.
          </p>
        </div>
        <div className="text-sm text-muted-foreground">
          {orders.totalCount} total order(s)
        </div>
      </div>

      {orders.items.length ? (
        <div className="space-y-4">
          {orders.items.map((order) => (
            <Link
              key={order.id}
              href={`${accountPath}/orders/${order.id}`}
              className="block border border-border hover:bg-secondary/30 transition-colors"
            >
              <div className="flex flex-col gap-6 p-6 lg:flex-row lg:items-center lg:justify-between">
                <div className="flex items-start gap-4">
                  <div className="mt-1 flex h-12 w-12 items-center justify-center bg-secondary">
                    <Package className="h-5 w-5" strokeWidth={1} />
                  </div>
                  <div className="space-y-3">
                    <div>
                      <p className="font-medium tracking-wide">{order.orderNumber}</p>
                      <p className="text-sm text-muted-foreground">
                        Placed on {formatDate(order.placedAtUtc)}
                      </p>
                    </div>
                    <div className="flex flex-wrap gap-2 text-xs tracking-[0.15em] uppercase">
                      <span className="bg-secondary px-3 py-1 text-muted-foreground">
                        {humanizeToken(order.status)}
                      </span>
                      <span className="bg-secondary px-3 py-1 text-muted-foreground">
                        Payment: {humanizeToken(order.paymentStatus)}
                      </span>
                      <span className="bg-secondary px-3 py-1 text-muted-foreground">
                        Fulfillment: {humanizeToken(order.fulfillmentStatus)}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="text-left lg:text-right">
                  <p className="font-medium tracking-wide">
                    {formatMoney(order.grandTotalAmount, order.currencyCode)}
                  </p>
                  <p className="text-sm text-muted-foreground">{order.itemCount} item(s)</p>
                </div>
              </div>
            </Link>
          ))}
        </div>
      ) : (
        <div className="border border-border p-8 text-sm text-muted-foreground">
          No orders were found for this account yet.
        </div>
      )}

      {orders.totalPages > 1 ? (
        <div className="flex items-center justify-between border-t border-border pt-6">
          <Link
            href={orders.hasPreviousPage ? `${accountPath}/orders?page=${orders.pageNumber - 1}` : "#"}
            aria-disabled={!orders.hasPreviousPage}
            className={`inline-flex items-center gap-2 text-sm transition-colors ${
              orders.hasPreviousPage
                ? "text-muted-foreground hover:text-foreground"
                : "pointer-events-none text-muted-foreground/40"
            }`}
          >
            <ChevronLeft className="h-4 w-4" strokeWidth={1} />
            Previous
          </Link>

          <p className="text-sm text-muted-foreground">
            Page {orders.pageNumber} of {orders.totalPages}
          </p>

          <Link
            href={orders.hasNextPage ? `${accountPath}/orders?page=${orders.pageNumber + 1}` : "#"}
            aria-disabled={!orders.hasNextPage}
            className={`inline-flex items-center gap-2 text-sm transition-colors ${
              orders.hasNextPage
                ? "text-muted-foreground hover:text-foreground"
                : "pointer-events-none text-muted-foreground/40"
            }`}
          >
            Next
            <ChevronRight className="h-4 w-4" strokeWidth={1} />
          </Link>
        </div>
      ) : null}
    </div>
  )
}
