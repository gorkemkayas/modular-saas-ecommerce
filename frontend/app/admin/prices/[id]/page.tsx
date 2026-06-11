import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPriceEntryManager } from "@/components/admin/admin-price-entry-manager"
import { AdminPriceListActions } from "@/components/admin/admin-price-list-actions"
import { getPriceListById, getProductById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime } from "@/lib/admin-format"

export default async function AdminPriceDetailPage({ params }: { params: { id: string } }) {
  try {
    const priceList = await getPriceListById(params.id)
    const uniqueProductIds = [...new Set(priceList.entries.map((entry) => entry.productId))]
    const products = await Promise.all(
      uniqueProductIds.map(async (productId) => {
        const product = await getProductById(productId)
        return [productId, product] as const
      }),
    )
    const productMap = new Map(products)
    const entryItems = priceList.entries.map((entry) => {
      const product = productMap.get(entry.productId)
      const variant = product?.variants.find(
        (candidate) => candidate.id === entry.productVariantId,
      )

      return {
        entry,
        productName: product?.name ?? entry.productId,
        productSlug: product?.slug ?? entry.productId,
        variantName: variant?.name ?? null,
        variantSku: variant?.sku ?? null,
      }
    })

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/prices" className="hover:text-foreground">
            Price Lists
          </Link>
          <span>/</span>
          <span className="text-foreground">{priceList.name}</span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">{priceList.name}</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            {priceList.currencyCode} • {priceList.status}
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-4">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Default</p>
            <p className="mt-2 text-sm">{priceList.isDefault ? "Yes" : "No"}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Priority</p>
            <p className="mt-2 text-sm">{priceList.priority}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Created</p>
            <p className="mt-2 text-sm">{formatDateTime(priceList.createdAtUtc)}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Updated</p>
            <p className="mt-2 text-sm">{formatDateTime(priceList.updatedAtUtc)}</p>
          </div>
        </div>

        <AdminPriceEntryManager
          priceListId={priceList.id}
          currencyCode={priceList.currencyCode}
          entries={entryItems}
        />

        <AdminPriceListActions
          priceListId={priceList.id}
          initialName={priceList.name}
          initialPriority={priceList.priority}
          status={priceList.status}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Price list detail could not be loaded"
        message={getApiErrorMessage(error, "The price list detail request failed.")}
      />
    )
  }
}
