import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchCustomers } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

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

export default async function CustomersPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q")
  const status = getValue(resolvedSearchParams, "status")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchCustomers({
      searchTerm: query || undefined,
      status: status || undefined,
      pageNumber: page,
      pageSize: 15,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Customers</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Customer management is connected to the tenant admin customer endpoints.
          </p>
        </div>

        <form className="grid gap-4 border border-border p-4 md:grid-cols-3">
          <input
            type="text"
            name="q"
            defaultValue={query}
            placeholder="Search by name, email or phone"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <select
            name="status"
            defaultValue={status}
            className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          >
            <option value="">All statuses</option>
            <option value="Active">Active</option>
            <option value="Blocked">Blocked</option>
          </select>
          <button className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90">
            Apply Filters
          </button>
        </form>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Customer</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Contact</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Addresses</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Registered</th>
                  <th className="p-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Detail</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((customer) => (
                  <tr key={customer.id} className="hover:bg-secondary/30">
                    <td className="p-4">
                      <p className="text-sm font-medium">{customer.fullName}</p>
                      <p className="text-xs text-muted-foreground">{customer.email}</p>
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {customer.phoneNumber ?? "No phone"}
                    </td>
                    <td className="p-4 text-sm">{formatEnumLabel(customer.status)}</td>
                    <td className="p-4 text-sm">{customer.addressCount}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(customer.registeredAtUtc)}
                    </td>
                    <td className="p-4 text-right">
                      <Link href={`/admin/customers/${customer.id}`} className="text-sm hover:text-muted-foreground">
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
          basePath="/admin/customers"
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{ q: query, status }}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Customers could not be loaded"
        message={getApiErrorMessage(error, "The customer search request failed.")}
      />
    )
  }
}
