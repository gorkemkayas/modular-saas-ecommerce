import {
  AlertTriangle,
  ArrowRight,
  CreditCard,
  Package,
  Truck,
  Users,
} from "lucide-react"
import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import {
  getStoreSettings,
  searchCustomers,
  searchInventoryItems,
  searchProducts,
  searchStorePayments,
  searchStoreShipments,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel, formatMoney } from "@/lib/admin-format"

const statCards = [
  {
    key: "products",
    name: "Products",
    icon: Package,
    description: "Catalog items currently indexed in backend",
  },
  {
    key: "customers",
    name: "Customers",
    icon: Users,
    description: "Registered customers in this tenant",
  },
  {
    key: "payments",
    name: "Payments",
    icon: CreditCard,
    description: "Payment records returned by backend reporting",
  },
  {
    key: "lowStock",
    name: "Low Stock",
    icon: AlertTriangle,
    description: "Inventory items under reorder threshold",
  },
] as const

function getPaymentStatusClasses(status: string): string {
  switch (status) {
    case "Captured":
    case "Authorized":
    case "Refunded":
      return "bg-foreground text-background"
    case "Pending":
      return "bg-secondary text-foreground"
    case "Failed":
    case "Cancelled":
      return "border border-border text-muted-foreground"
    default:
      return "bg-secondary text-foreground"
  }
}

function getShipmentStatusClasses(status: string): string {
  switch (status) {
    case "Delivered":
      return "bg-foreground text-background"
    case "Shipped":
    case "ReadyForDispatch":
      return "bg-secondary text-foreground"
    case "Cancelled":
      return "border border-border text-muted-foreground"
    default:
      return "bg-secondary text-foreground"
  }
}

export default async function AdminDashboard() {
  try {
    const [store, products, customers, payments, lowStock, shipments] = await Promise.all([
      getStoreSettings(),
      searchProducts({ pageNumber: 1, pageSize: 5 }),
      searchCustomers({ pageNumber: 1, pageSize: 5 }),
      searchStorePayments({ pageNumber: 1, pageSize: 5 }),
      searchInventoryItems({ pageNumber: 1, pageSize: 5, onlyLowStock: true }),
      searchStoreShipments({ pageNumber: 1, pageSize: 5 }),
    ])

    const statValues = {
      products: products.totalCount,
      customers: customers.totalCount,
      payments: payments.totalCount,
      lowStock: lowStock.totalCount,
    } as const

    return (
      <div className="space-y-8">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Dashboard</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Live tenant overview for {store.name}, based on current backend admin endpoints.
          </p>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {statCards.map((stat) => {
            const Icon = stat.icon

            return (
              <div key={stat.key} className="border border-border bg-card p-6">
                <div className="flex items-center justify-between">
                  <div className="bg-secondary p-2">
                    <Icon className="h-5 w-5" strokeWidth={1.5} />
                  </div>
                </div>
                <div className="mt-4">
                  <p className="text-2xl font-light">{statValues[stat.key]}</p>
                  <p className="mt-1 text-xs uppercase tracking-wide text-muted-foreground">
                    {stat.name}
                  </p>
                  <p className="mt-2 text-xs text-muted-foreground">{stat.description}</p>
                </div>
              </div>
            )
          })}
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="border border-border bg-card">
            <div className="flex items-center justify-between border-b border-border p-6">
              <h2 className="font-light tracking-wide">Recent Payments</h2>
              <Link
                href="/admin/payments"
                className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
              >
                View All <ArrowRight className="h-3 w-3" strokeWidth={1.5} />
              </Link>
            </div>
            <div className="divide-y divide-border">
              {payments.items.length ? (
                payments.items.map((payment) => (
                  <div
                    key={payment.id}
                    className="flex items-center justify-between gap-4 p-4 transition-colors hover:bg-secondary/50"
                  >
                    <div>
                      <p className="text-sm font-medium">{payment.orderNumber}</p>
                      <p className="text-xs text-muted-foreground">
                        {formatEnumLabel(payment.provider)} · {formatEnumLabel(payment.methodType)}
                      </p>
                    </div>
                    <div className="flex items-center gap-4">
                      <span
                        className={`px-2 py-1 text-[10px] uppercase tracking-wider ${getPaymentStatusClasses(payment.status)}`}
                      >
                        {formatEnumLabel(payment.status)}
                      </span>
                      <div className="text-right">
                        <p className="text-sm">
                          {formatMoney(payment.amount, payment.currencyCode)}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {formatDateTime(payment.createdAtUtc)}
                        </p>
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="p-4 text-sm text-muted-foreground">
                  No payment records have been returned by the backend yet.
                </p>
              )}
            </div>
          </div>

          <div className="border border-border bg-card">
            <div className="flex items-center justify-between border-b border-border p-6">
              <h2 className="font-light tracking-wide">Shipment Activity</h2>
              <Link
                href="/admin/shipments"
                className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
              >
                View All <ArrowRight className="h-3 w-3" strokeWidth={1.5} />
              </Link>
            </div>
            <div className="divide-y divide-border">
              {shipments.items.length ? (
                shipments.items.map((shipment) => (
                  <div
                    key={shipment.id}
                    className="flex items-center justify-between gap-4 p-4 transition-colors hover:bg-secondary/50"
                  >
                    <div>
                      <p className="text-sm font-medium">{shipment.shipmentNumber}</p>
                      <p className="text-xs text-muted-foreground">
                        {shipment.orderNumber} · {shipment.recipientName}
                      </p>
                    </div>
                    <div className="flex items-center gap-4">
                      <span
                        className={`px-2 py-1 text-[10px] uppercase tracking-wider ${getShipmentStatusClasses(shipment.status)}`}
                      >
                        {formatEnumLabel(shipment.status)}
                      </span>
                      <div className="text-right">
                        <p className="text-sm">{shipment.carrierName ?? "Carrier pending"}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatDateTime(
                            shipment.deliveredAtUtc ??
                              shipment.shippedAtUtc ??
                              shipment.createdAtUtc,
                          )}
                        </p>
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="p-4 text-sm text-muted-foreground">
                  No shipment records have been returned by the backend yet.
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="border border-border bg-card">
            <div className="flex items-center justify-between border-b border-border p-6">
              <h2 className="font-light tracking-wide">Latest Products</h2>
              <Link
                href="/admin/products"
                className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
              >
                View All <ArrowRight className="h-3 w-3" strokeWidth={1.5} />
              </Link>
            </div>
            <div className="divide-y divide-border">
              {products.items.length ? (
                products.items.map((product) => (
                  <div
                    key={product.id}
                    className="flex items-center justify-between gap-4 p-4 transition-colors hover:bg-secondary/50"
                  >
                    <div>
                      <p className="text-sm font-medium">{product.name}</p>
                      <p className="text-xs text-muted-foreground">/{product.slug}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm">{formatEnumLabel(product.productType)}</p>
                      <p className="text-xs text-muted-foreground">
                        {formatEnumLabel(product.productStatus)} ·{" "}
                        {product.isPublished ? "Published" : "Private"}
                      </p>
                    </div>
                  </div>
                ))
              ) : (
                <p className="p-4 text-sm text-muted-foreground">
                  No product records have been returned by the backend yet.
                </p>
              )}
            </div>
          </div>

          <div className="border border-border bg-card">
            <div className="flex items-center justify-between border-b border-border p-6">
              <h2 className="font-light tracking-wide">Recent Customers</h2>
              <Link
                href="/admin/customers"
                className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
              >
                View All <ArrowRight className="h-3 w-3" strokeWidth={1.5} />
              </Link>
            </div>
            <div className="divide-y divide-border">
              {customers.items.length ? (
                customers.items.map((customer) => (
                  <div
                    key={customer.id}
                    className="flex items-center justify-between gap-4 p-4 transition-colors hover:bg-secondary/50"
                  >
                    <div>
                      <p className="text-sm font-medium">{customer.fullName}</p>
                      <p className="text-xs text-muted-foreground">{customer.email}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm">{formatEnumLabel(customer.status)}</p>
                      <p className="text-xs text-muted-foreground">
                        {formatDateTime(customer.registeredAtUtc)}
                      </p>
                    </div>
                  </div>
                ))
              ) : (
                <p className="p-4 text-sm text-muted-foreground">
                  No customer records have been returned by the backend yet.
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {[
            { name: "Add Product", href: "/admin/products/create", icon: Package },
            { name: "Payments", href: "/admin/payments", icon: CreditCard },
            { name: "Shipments", href: "/admin/shipments", icon: Truck },
            { name: "Customers", href: "/admin/customers", icon: Users },
          ].map((action) => {
            const Icon = action.icon

            return (
              <Link
                key={action.name}
                href={action.href}
                className="group flex items-center gap-3 border border-border p-4 transition-colors hover:bg-secondary"
              >
                <Icon
                  className="h-5 w-5 text-muted-foreground transition-colors group-hover:text-foreground"
                  strokeWidth={1.5}
                />
                <span className="text-sm">{action.name}</span>
              </Link>
            )
          })}
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Dashboard could not be loaded"
        message={getApiErrorMessage(error, "The dashboard request failed.")}
      />
    )
  }
}
