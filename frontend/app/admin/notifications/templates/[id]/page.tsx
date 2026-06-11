import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminNotificationTemplateActions } from "@/components/admin/admin-notification-template-actions"
import { getNotificationTemplateById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

export default async function AdminNotificationTemplateDetailPage({ params }: { params: { id: string } }) {
  try {
    const template = await getNotificationTemplateById(params.id)

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/notifications/templates" className="hover:text-foreground">
            Templates
          </Link>
          <span>/</span>
          <span className="text-foreground">{template.name}</span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">{template.name}</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            {formatEnumLabel(template.trigger)} • {formatEnumLabel(template.channel)} • {template.locale}
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-4">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Status</p>
            <p className="mt-2 text-sm">{template.isActive ? "Active" : "Inactive"}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Locale</p>
            <p className="mt-2 text-sm">{template.locale}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Created</p>
            <p className="mt-2 text-sm">{formatDateTime(template.createdAtUtc)}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Updated</p>
            <p className="mt-2 text-sm">{formatDateTime(template.updatedAtUtc)}</p>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Subject Template</h2>
          <pre className="mt-4 whitespace-pre-wrap text-sm text-muted-foreground">
            {template.subjectTemplate}
          </pre>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Body Template</h2>
          <pre className="mt-4 whitespace-pre-wrap text-sm text-muted-foreground">
            {template.bodyTemplate}
          </pre>
        </div>

        <AdminNotificationTemplateActions
          templateId={template.id}
          initialLocale={template.locale}
          initialName={template.name}
          initialSubject={template.subjectTemplate}
          initialBody={template.bodyTemplate}
          isActive={template.isActive}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Notification template detail could not be loaded"
        message={getApiErrorMessage(error, "The notification template detail request failed.")}
      />
    )
  }
}
