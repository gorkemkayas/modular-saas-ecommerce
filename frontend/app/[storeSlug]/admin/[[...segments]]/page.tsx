import { notFound, redirect } from "next/navigation"

import AdminAttributesPage from "@/app/admin/attributes/page"
import AdminBrandsPage from "@/app/admin/brands/page"
import AdminCategoriesPage from "@/app/admin/categories/page"
import AdminCustomersPage from "@/app/admin/customers/page"
import AdminCustomerDetailPage from "@/app/admin/customers/[id]/page"
import AdminDashboard from "@/app/admin/page"
import AdminInventoryPage from "@/app/admin/inventory/page"
import AdminInventoryDetailPage from "@/app/admin/inventory/[id]/page"
import AdminInventoryMovementsPage from "@/app/admin/inventory/[id]/movements/page"
import AdminNotificationsPage from "@/app/admin/notifications/page"
import AdminNotificationHistoryPage from "@/app/admin/notifications/history/page"
import AdminNotificationTemplatesPage from "@/app/admin/notifications/templates/page"
import AdminNotificationTemplateDetailPage from "@/app/admin/notifications/templates/[id]/page"
import AdminOrdersPage from "@/app/admin/orders/page"
import AdminOrderDetailPage from "@/app/admin/orders/[id]/page"
import AdminPaymentsPage from "@/app/admin/payments/page"
import AdminPaymentDetailPage from "@/app/admin/payments/[id]/page"
import AdminPaymentSettingsPage from "@/app/admin/payment-settings/page"
import AdminPricesPage from "@/app/admin/prices/page"
import AdminPriceDetailPage from "@/app/admin/prices/[id]/page"
import AdminProductsPage from "@/app/admin/products/page"
import AdminProductCreatePage from "@/app/admin/products/create/page"
import AdminProductEditQueryPage from "@/app/admin/products/edit/page"
import AdminProductDetailPage from "@/app/admin/products/[id]/page"
import AdminPublishPage from "@/app/admin/publish/page"
import AdminShipmentsPage from "@/app/admin/shipments/page"
import AdminShipmentDetailPage from "@/app/admin/shipments/[id]/page"
import AdminStockMovementsPage from "@/app/admin/stock-movements/page"
import AdminStoreSettingsPage from "@/app/admin/store-settings/page"
import AdminStoreSettingsSlugPage from "@/app/admin/store-settings/slug/page"
import AdminSubscriptionPage from "@/app/admin/subscription/page"
import { getAdminPath } from "@/lib/admin-path"

type Props = {
  params: Promise<{ storeSlug: string; segments?: string[] }>
  searchParams?: Promise<Record<string, string | string[] | undefined>>
}

export default async function StoreAdminRouter(props: Props) {
  const { storeSlug, segments = [] } = await props.params
  const searchParams = props.searchParams

  if (segments.length === 0) {
    return <AdminDashboard />
  }

  if (segments.length === 1) {
    switch (segments[0]) {
      case "attributes":
        return <AdminAttributesPage searchParams={searchParams} />
      case "brands":
        return <AdminBrandsPage searchParams={searchParams} />
      case "categories":
        return <AdminCategoriesPage searchParams={searchParams} />
      case "customers":
        return <AdminCustomersPage searchParams={searchParams} />
      case "inventory":
        return <AdminInventoryPage searchParams={searchParams} />
      case "notifications":
        return <AdminNotificationsPage />
      case "orders":
        return <AdminOrdersPage />
      case "payments":
        return <AdminPaymentsPage searchParams={searchParams} />
      case "payment-settings":
        return <AdminPaymentSettingsPage />
      case "prices":
        return <AdminPricesPage searchParams={searchParams} />
      case "products":
        return <AdminProductsPage searchParams={searchParams} />
      case "publish":
        return <AdminPublishPage />
      case "shipments":
        return <AdminShipmentsPage searchParams={searchParams} />
      case "stock-movements":
        return <AdminStockMovementsPage searchParams={searchParams} />
      case "store-settings":
        return <AdminStoreSettingsPage />
      case "subscription":
        return <AdminSubscriptionPage />
      default:
        notFound()
    }
  }

  if (segments.length === 2) {
    switch (`${segments[0]}/${segments[1]}`) {
      case "notifications/history":
        return <AdminNotificationHistoryPage searchParams={searchParams} />
      case "notifications/templates":
        return <AdminNotificationTemplatesPage searchParams={searchParams} />
      case "products/create":
        return <AdminProductCreatePage />
      case "products/edit": {
        const resolvedSearchParams = searchParams ? await searchParams : {}
        const id = typeof resolvedSearchParams.id === "string" ? resolvedSearchParams.id : undefined
        return <AdminProductEditQueryPage searchParams={Promise.resolve({ id })} />
      }
      case "store-settings/slug":
        return <AdminStoreSettingsSlugPage />
      default:
        switch (segments[0]) {
          case "customers":
            return <AdminCustomerDetailPage params={{ id: segments[1] }} />
          case "inventory":
            return <AdminInventoryDetailPage params={{ id: segments[1] }} />
          case "orders":
            return <AdminOrderDetailPage params={{ id: segments[1] }} />
          case "payments":
            return <AdminPaymentDetailPage params={{ id: segments[1] }} />
          case "prices":
            return <AdminPriceDetailPage params={{ id: segments[1] }} />
          case "products":
            return <AdminProductDetailPage params={{ id: segments[1] }} />
          case "shipments":
            return <AdminShipmentDetailPage params={{ id: segments[1] }} />
          default:
            notFound()
        }
    }
  }

  if (segments.length === 3) {
    if (segments[0] === "inventory" && segments[2] === "movements") {
      return (
        <AdminInventoryMovementsPage
          params={{ id: segments[1] }}
          searchParams={searchParams}
        />
      )
    }

    if (segments[0] === "notifications" && segments[1] === "templates") {
      return <AdminNotificationTemplateDetailPage params={{ id: segments[2] }} />
    }

    if (segments[0] === "products" && segments[2] === "edit") {
      redirect(`${getAdminPath(storeSlug, "/products/edit")}?id=${segments[1]}`)
    }
  }

  notFound()
}
