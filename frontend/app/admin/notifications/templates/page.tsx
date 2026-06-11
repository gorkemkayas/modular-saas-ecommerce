import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminNotificationTemplateCreateForm } from "@/components/admin/admin-create-forms"
import { searchNotificationTemplates } from "@/lib/api/admin"
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

export default async function AdminNotificationTemplatesPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const trigger = getValue(resolvedSearchParams, "trigger")
  const channel = getValue(resolvedSearchParams, "channel")
  const active = getValue(resolvedSearchParams, "active")

  try {
    const templates = await searchNotificationTemplates({
      trigger: trigger || undefined,
      channel: channel || undefined,
      isActive: active === "true" ? true : active === "false" ? false : undefined,
    })

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Notification Templates</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            The template list is now powered by `api/stores/me/notification-templates`.
          </p>
        </div>

        <AdminNotificationTemplateCreateForm />

        <form className="grid gap-4 border border-border p-4 md:grid-cols-4">
          <input
            type="text"
            name="trigger"
            defaultValue={trigger}
            placeholder="Trigger, e.g. OrderCreated"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <input
            type="text"
            name="channel"
            defaultValue={channel}
            placeholder="Channel, e.g. Email"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
          <select
            name="active"
            defaultValue={active}
            className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          >
            <option value="">All states</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
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
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Template</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Trigger</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Channel</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Locale</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Updated</th>
                  <th className="px-6 py-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Detail</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {templates.map((template) => (
                  <tr key={template.id} className="hover:bg-secondary/30">
                    <td className="px-6 py-4 text-sm font-medium">{template.name}</td>
                    <td className="px-6 py-4 text-sm">{formatEnumLabel(template.trigger)}</td>
                    <td className="px-6 py-4 text-sm">{formatEnumLabel(template.channel)}</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">{template.locale}</td>
                    <td className="px-6 py-4 text-sm">{template.isActive ? "Active" : "Inactive"}</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">
                      {formatDateTime(template.updatedAtUtc)}
                    </td>
                    <td className="px-6 py-4 text-right">
                      <Link
                        href={`/admin/notifications/templates/${template.id}`}
                        className="text-sm hover:text-muted-foreground"
                      >
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Notification templates could not be loaded"
        message={getApiErrorMessage(error, "The notification template search request failed.")}
      />
    )
  }
}
