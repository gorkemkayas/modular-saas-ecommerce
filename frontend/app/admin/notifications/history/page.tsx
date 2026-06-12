import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPagination } from "@/components/admin/admin-pagination"
import { searchNotificationDispatches } from "@/lib/api/admin"
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

export default async function AdminNotificationHistoryPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const status = getValue(resolvedSearchParams, "status")
  const channel = getValue(resolvedSearchParams, "channel")
  const trigger = getValue(resolvedSearchParams, "trigger")
  const page = getPage(resolvedSearchParams)

  try {
    const result = await searchNotificationDispatches({
      status: status || undefined,
      channel: channel || undefined,
      trigger: trigger || undefined,
      pageNumber: page,
      pageSize: 15,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Notification History</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Dispatch history is backed by `api/stores/me/notifications`.
          </p>
        </div>

        <form className="grid gap-4 border border-border p-4 md:grid-cols-4">
          <input
            type="text"
            name="trigger"
            defaultValue={trigger}
            placeholder="Trigger"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <input
            type="text"
            name="channel"
            defaultValue={channel}
            placeholder="Channel"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <input
            type="text"
            name="status"
            defaultValue={status}
            placeholder="Status"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <button className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90">
            Apply Filters
          </button>
        </form>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Recipient</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Trigger</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Channel</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Provider</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Created</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {result.items.map((dispatch) => (
                  <tr key={dispatch.id} className="hover:bg-secondary/30">
                    <td className="px-6 py-4 text-sm">
                      {dispatch.recipientAddress ?? "No recipient address"}
                    </td>
                    <td className="px-6 py-4 text-sm">{formatEnumLabel(dispatch.trigger)}</td>
                    <td className="px-6 py-4 text-sm">{formatEnumLabel(dispatch.channel)}</td>
                    <td className="px-6 py-4 text-sm">{formatEnumLabel(dispatch.status)}</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">
                      {dispatch.providerName ?? "Not assigned"}
                    </td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">
                      {formatDateTime(dispatch.createdAtUtc)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <AdminPagination
          basePath="/admin/notifications/history"
          currentPage={result.pageNumber}
          totalPages={result.totalPages}
          query={{ status, channel, trigger }}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Notification history could not be loaded"
        message={getApiErrorMessage(error, "The notification dispatch search request failed.")}
      />
    )
  }
}
