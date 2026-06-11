import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPriceListCreateForm } from "@/components/admin/admin-create-forms"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchPriceLists } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { getCurrentSubscriptionOrNull } from "@/lib/api/subscription"
import { formatDateTime } from "@/lib/admin-format"

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>
}

function getValue(
  searchParams: Record<string, string | string[] | undefined>,
  key: string,
): string {
  const value = searchParams[key]
  return typeof value === "string" ? value : ""
}

function getPage(searchParams: Record<string, string | string[] | undefined>): number {
  const rawValue = getValue(searchParams, "page")
  const parsedValue = Number.parseInt(rawValue, 10)
  return Number.isFinite(parsedValue) && parsedValue > 0 ? parsedValue : 1
}

export default async function PricesPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const currency = getValue(resolvedSearchParams, "currency")
  const status = getValue(resolvedSearchParams, "status")
  const page = getPage(resolvedSearchParams)

  try {
    const [result, subscription, draftLists, activeLists, inactiveLists] = await Promise.all([
      searchPriceLists({
        currencyCode: currency || undefined,
        status: status || undefined,
        pageNumber: page,
        pageSize: 12,
      }),
      getCurrentSubscriptionOrNull(),
      searchPriceLists({ status: "Draft", pageNumber: 1, pageSize: 1 }),
      searchPriceLists({ status: "Active", pageNumber: 1, pageSize: 1 }),
      searchPriceLists({ status: "Inactive", pageNumber: 1, pageSize: 1 }),
    ])
    const currentPriceListCount =
      draftLists.totalCount + activeLists.totalCount + inactiveLists.totalCount

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Price Lists</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Pricing filters now mirror the backend contract exactly: currency and list status.
          </p>
        </div>

        <AdminPriceListCreateForm
          subscription={subscription}
          currentPriceListCount={currentPriceListCount}
        />

        <form className="grid gap-4 border border-border p-4 md:grid-cols-3">
          <input
            type="text"
            name="currency"
            defaultValue={currency}
            placeholder="Currency code, e.g. TRY"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <select
            name="status"
            defaultValue={status}
            className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          >
            <option value="">All statuses</option>
            <option value="Draft">Draft</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Archived">Archived</option>
          </select>
          <button className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90">
            Apply Filters
          </button>
        </form>

        <div className="grid gap-4 lg:grid-cols-2">
          {result.items.map((priceList) => (
            <Link
              key={priceList.id}
              href={`/admin/prices/${priceList.id}`}
              className="border border-border p-6 transition-colors hover:bg-secondary/30"
            >
              <div className="flex items-start justify-between gap-4">
                <div>
                  <h2 className="text-lg font-light">{priceList.name}</h2>
                  <p className="mt-1 text-sm text-muted-foreground">
                    {priceList.currencyCode} • priority {priceList.priority}
                  </p>
                </div>
                <span className="text-xs uppercase tracking-wider text-muted-foreground">
                  {priceList.status}
                </span>
              </div>
              <div className="mt-6 grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Default</p>
                  <p>{priceList.isDefault ? "Yes" : "No"}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-wider text-muted-foreground">Updated</p>
                  <p>{formatDateTime(priceList.updatedAtUtc)}</p>
                </div>
              </div>
            </Link>
          ))}
        </div>

        <AdminPagination
          basePath="/admin/prices"
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{ currency, status }}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Price lists could not be loaded"
        message={getApiErrorMessage(error, "The price list search request failed.")}
      />
    )
  }
}
