import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchStorePayments } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel, formatMoney } from "@/lib/admin-format"

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

export default async function PaymentsPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const status = getValue(resolvedSearchParams, "status")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchStorePayments({
      status: status || undefined,
      pageNumber: page,
      pageSize: 15,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Payments</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Payment reporting now uses the backend payment summary contract directly.
          </p>
        </div>

        <form className="grid max-w-md gap-4 border border-border p-4">
          <select
            name="status"
            defaultValue={status}
            className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          >
            <option value="">All statuses</option>
            <option value="Pending">Pending</option>
            <option value="Authorized">Authorized</option>
            <option value="Captured">Captured</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Failed">Failed</option>
            <option value="Refunded">Refunded</option>
          </select>
          <button className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90">
            Apply Filters
          </button>
        </form>

        <div className="border border-border overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Payment</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Order</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Amount</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Method</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Created</th>
                  <th className="p-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Detail</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((payment) => (
                  <tr key={payment.id} className="hover:bg-secondary/30">
                    <td className="p-4">
                      <p className="text-sm font-medium">{payment.id}</p>
                      <p className="text-xs text-muted-foreground">{formatEnumLabel(payment.provider)}</p>
                    </td>
                    <td className="p-4 text-sm">{payment.orderNumber}</td>
                    <td className="p-4 text-sm">
                      {formatMoney(payment.amount, payment.currencyCode)}
                    </td>
                    <td className="p-4 text-sm">{formatEnumLabel(payment.methodType)}</td>
                    <td className="p-4 text-sm">{formatEnumLabel(payment.status)}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(payment.createdAtUtc)}
                    </td>
                    <td className="p-4 text-right">
                      <Link href={`/admin/payments/${payment.id}`} className="text-sm hover:text-muted-foreground">
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <AdminPagination
          basePath="/admin/payments"
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{ status }}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Payments could not be loaded"
        message={getApiErrorMessage(error, "The payment search request failed.")}
      />
    )
  }
}
