"use client"

import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  activateNotificationTemplate,
  deactivateNotificationTemplate,
  updateNotificationTemplate,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export function AdminNotificationTemplateActions({
  templateId,
  initialLocale,
  initialName,
  initialSubject,
  initialBody,
  isActive,
}: {
  templateId: string
  initialLocale: string
  initialName: string
  initialSubject: string
  initialBody: string
  isActive: boolean
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [locale, setLocale] = useState(initialLocale)
  const [name, setName] = useState(initialName)
  const [subjectTemplate, setSubjectTemplate] = useState(initialSubject)
  const [bodyTemplate, setBodyTemplate] = useState(initialBody)
  const [error, setError] = useState<string | null>(null)

  function run(action: () => Promise<void>) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        router.refresh()
      } catch (actionError) {
        setError(
          getApiErrorMessage(actionError, "The notification template action failed."),
        )
      }
    })
  }

  return (
    <div className="space-y-6 border border-border p-6">
      <h2 className="text-lg font-light tracking-wide">Template Actions</h2>
      <div className="space-y-3">
        <label className="block text-sm">Name</label>
        <input
          value={name}
          onChange={(event) => setName(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
        <label className="block text-sm">Locale</label>
        <input
          value={locale}
          onChange={(event) => setLocale(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
        <label className="block text-sm">Subject</label>
        <input
          value={subjectTemplate}
          onChange={(event) => setSubjectTemplate(event.target.value)}
          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
        <label className="block text-sm">Body</label>
        <textarea
          rows={8}
          value={bodyTemplate}
          onChange={(event) => setBodyTemplate(event.target.value)}
          className="w-full resize-none bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
        />
      </div>
      <div className="grid gap-3 sm:grid-cols-2">
        <button
          type="button"
          disabled={isPending}
          onClick={() =>
            run(() =>
              updateNotificationTemplate(templateId, {
                locale,
                name,
                subjectTemplate,
                bodyTemplate,
              }),
            )
          }
          className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
        >
          Save Template
        </button>
        {isActive ? (
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => deactivateNotificationTemplate(templateId))}
            className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Deactivate
          </button>
        ) : (
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => activateNotificationTemplate(templateId))}
            className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Activate
          </button>
        )}
      </div>
      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
          {error}
        </div>
      ) : null}
    </div>
  )
}
