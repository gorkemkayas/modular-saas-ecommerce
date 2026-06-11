import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { searchNotificationDispatches, searchNotificationTemplates } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatEnumLabel } from "@/lib/admin-format"

export default async function NotificationsPage() {
  try {
    const [templates, dispatches] = await Promise.all([
      searchNotificationTemplates({}),
      searchNotificationDispatches({ pageNumber: 1, pageSize: 20 }),
    ])

    const sentCount = dispatches.items.filter(
      (dispatch) => dispatch.status === "Sent" || dispatch.status === "Delivered",
    ).length

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Notifications</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            This hub now reflects the actual backend notification model: templates plus dispatch history.
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-3">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Templates</p>
            <p className="mt-2 text-3xl font-light">{templates.length}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Recent dispatches</p>
            <p className="mt-2 text-3xl font-light">{dispatches.items.length}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Successful sends</p>
            <p className="mt-2 text-3xl font-light">{sentCount}</p>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <Link
            href="/admin/notifications/templates"
            className="border border-border p-6 transition-colors hover:bg-secondary/30"
          >
            <h2 className="text-lg font-light tracking-wide">Notification Templates</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Channel-specific templates keyed by trigger, locale and active state.
            </p>
          </Link>
          <Link
            href="/admin/notifications/history"
            className="border border-border p-6 transition-colors hover:bg-secondary/30"
          >
            <h2 className="text-lg font-light tracking-wide">Dispatch History</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Provider events, send outcomes and recipient-level delivery details.
            </p>
          </Link>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Latest Activity</h2>
          <div className="mt-4 space-y-3">
            {dispatches.items.slice(0, 5).map((dispatch) => (
              <div key={dispatch.id} className="flex items-center justify-between gap-4 border-b border-border pb-3 text-sm last:border-b-0 last:pb-0">
                <div>
                  <p>
                    {formatEnumLabel(dispatch.trigger)} via {formatEnumLabel(dispatch.channel)}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {dispatch.recipientAddress ?? dispatch.businessEntityType}
                  </p>
                </div>
                <span className="text-xs uppercase tracking-wider text-muted-foreground">
                  {formatEnumLabel(dispatch.status)}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Notifications could not be loaded"
        message={getApiErrorMessage(error, "The notification summary requests failed.")}
      />
    )
  }
}
