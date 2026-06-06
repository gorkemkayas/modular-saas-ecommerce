import Link from "next/link"
import { ArrowRight, MapPin, Package } from "lucide-react"
import { getMyOrders, getMyProfile } from "@/lib/api/account"
import { ApiError } from "@/lib/api/client"
import { getAccountPath } from "@/lib/account-path"
import { formatMoney } from "@/lib/format"

export default async function AccountPage({
  params,
}: {
  params?: Promise<{ storeSlug?: string }>
}) {
  const storeSlug = (await params)?.storeSlug
  const accountPath = getAccountPath(storeSlug)
  const [profileResult, ordersResult] = await Promise.allSettled([
    getMyProfile(),
    getMyOrders(1, 5),
  ])

  if (profileResult.status !== "fulfilled") {
    throw profileResult.reason
  }

  const profile = profileResult.value
  const orders =
    ordersResult.status === "fulfilled"
      ? ordersResult.value
      : null
  const ordersLoadFailed =
    ordersResult.status === "rejected" &&
    !(ordersResult.reason instanceof ApiError && ordersResult.reason.status === 404)

  return (
    <div className="space-y-12">
      <section>
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-xs tracking-[0.3em] uppercase">Profile Information</h2>
          <Link
            href={getAccountPath(storeSlug, "/preferences")}
            className="text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            Preferences
          </Link>
        </div>

        <div className="bg-secondary/30 p-8">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-8">
            <div>
              <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
                Full Name
              </p>
              <p className="text-lg tracking-wide">
                {profile.firstName} {profile.lastName}
              </p>
            </div>
            <div>
              <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
                Email
              </p>
              <p className="text-lg tracking-wide">{profile.email}</p>
            </div>
            <div>
              <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
                Phone
              </p>
              <p className="text-lg tracking-wide">{profile.phoneNumber || "-"}</p>
            </div>
            <div>
              <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
                Registered
              </p>
              <p className="text-lg tracking-wide">
                {new Date(profile.registeredAtUtc).toLocaleDateString("tr-TR")}
              </p>
            </div>
          </div>
        </div>
      </section>

      <section className="grid grid-cols-1 sm:grid-cols-2 gap-6">
        <Link
          href={getAccountPath(storeSlug, "/orders")}
          className="flex items-center justify-between p-8 border border-border hover:bg-secondary/30 transition-colors group"
        >
          <div className="flex items-center gap-4">
            <Package className="h-6 w-6" strokeWidth={1} />
            <div>
              <h3 className="font-medium tracking-wide">Order History</h3>
              <p className="text-sm text-muted-foreground">
                Review placed orders and payment status
              </p>
            </div>
          </div>
          <ArrowRight
            className="h-5 w-5 text-muted-foreground group-hover:translate-x-1 transition-transform"
            strokeWidth={1}
          />
        </Link>

        <Link
          href={getAccountPath(storeSlug, "/addresses")}
          className="flex items-center justify-between p-8 border border-border hover:bg-secondary/30 transition-colors group"
        >
          <div className="flex items-center gap-4">
            <MapPin className="h-6 w-6" strokeWidth={1} />
            <div>
              <h3 className="font-medium tracking-wide">Saved Addresses</h3>
              <p className="text-sm text-muted-foreground">
                Shipping and billing addresses from customer profile
              </p>
            </div>
          </div>
          <ArrowRight
            className="h-5 w-5 text-muted-foreground group-hover:translate-x-1 transition-transform"
            strokeWidth={1}
          />
        </Link>
      </section>

      <section>
        <div className="flex items-center justify-between mb-8">
          <h2 className="text-xs tracking-[0.3em] uppercase">Recent Orders</h2>
          <Link
            href={getAccountPath(storeSlug, "/orders")}
            className="text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            View All
          </Link>
        </div>

        <div className="space-y-4">
          {ordersLoadFailed ? (
            <div className="border border-border p-8 text-sm text-muted-foreground">
              We couldn&apos;t load recent orders right now. You can still manage your profile and try the orders page again shortly.
            </div>
          ) : orders && orders.items.length ? (
            orders.items.map((order) => (
              <Link
                key={order.id}
                href={`${accountPath}/orders/${order.id}`}
                className="flex items-center justify-between p-6 border border-border hover:bg-secondary/30 transition-colors"
              >
                <div className="flex items-center gap-8">
                  <div>
                    <p className="font-medium tracking-wide">{order.orderNumber}</p>
                    <p className="text-sm text-muted-foreground">
                      {new Date(order.placedAtUtc).toLocaleDateString("tr-TR")}
                    </p>
                  </div>
                  <span className="text-xs tracking-[0.2em] uppercase px-3 py-1 bg-secondary text-muted-foreground">
                    {order.fulfillmentStatus}
                  </span>
                </div>
                <div className="text-right">
                  <p className="font-medium tracking-wide">
                    {formatMoney(order.grandTotalAmount, order.currencyCode)}
                  </p>
                  <p className="text-sm text-muted-foreground">{order.itemCount} item(s)</p>
                </div>
              </Link>
            ))
          ) : (
            <div className="border border-border p-8 text-sm text-muted-foreground">
              No orders found for this account yet.
            </div>
          )}
        </div>
      </section>
    </div>
  )
}
