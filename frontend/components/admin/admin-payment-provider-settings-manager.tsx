"use client"

import { Save, Power } from "lucide-react"
import { useRouter } from "next/navigation"
import { useState, useTransition } from "react"

import {
  disableIyzicoPaymentProviderAccount,
  updateIyzicoPaymentProviderAccount,
} from "@/lib/api/admin"
import type { IyzicoPaymentProviderAccountDto } from "@/lib/api/types"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

function Notice({
  kind,
  message,
}: {
  kind: "error" | "success"
  message: string | null
}) {
  if (!message) {
    return null
  }

  const className =
    kind === "error"
      ? "border-destructive/30 bg-destructive/5 text-destructive"
      : "border-emerald-500/30 bg-emerald-500/5 text-emerald-700"

  return <div className={`border px-4 py-3 text-sm ${className}`}>{message}</div>
}

export function AdminPaymentProviderSettingsManager({
  initialAccount,
}: {
  initialAccount: IyzicoPaymentProviderAccountDto | null
}) {
  const router = useRouter()
  const [account, setAccount] = useState(initialAccount)
  const [isEnabled, setIsEnabled] = useState(initialAccount?.isEnabled ?? false)
  const [apiKey, setApiKey] = useState("")
  const [secretKey, setSecretKey] = useState("")
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const [isPending, startTransition] = useTransition()

  function resetMessages() {
    setError(null)
    setMessage(null)
  }

  function saveSettings() {
    resetMessages()

    startTransition(async () => {
      try {
        const updatedAccount = await updateIyzicoPaymentProviderAccount({
          apiKey: apiKey.trim() || null,
          secretKey: secretKey.trim() || null,
          isEnabled,
        })

        setAccount(updatedAccount)
        setApiKey("")
        setSecretKey("")
        setIsEnabled(updatedAccount.isEnabled)
        setMessage("Iyzico account settings were saved.")
        router.refresh()
      } catch (saveError) {
        setError(getApiErrorMessage(saveError, "Iyzico account settings could not be saved."))
      }
    })
  }

  function disableSettings() {
    resetMessages()

    startTransition(async () => {
      try {
        const disabledAccount = await disableIyzicoPaymentProviderAccount()
        setAccount(disabledAccount)
        setIsEnabled(false)
        setMessage("Iyzico account was disabled.")
        router.refresh()
      } catch (disableError) {
        setError(getApiErrorMessage(disableError, "Iyzico account could not be disabled."))
      }
    })
  }

  const canSave =
    (account !== null || (apiKey.trim().length > 0 && secretKey.trim().length > 0))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-light tracking-wide">Payment Settings</h1>
      </div>

      <Notice kind="error" message={error} />
      <Notice kind="success" message={message} />

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_320px]">
        <section className="border border-border p-6">
          <div className="grid gap-6">
            <div className="grid gap-5 md:grid-cols-2">
              <div>
                <label className="mb-2 block text-sm">API Key</label>
                <input
                  type="password"
                  value={apiKey}
                  onChange={(event) => setApiKey(event.target.value)}
                  placeholder={account?.apiKeyMasked ?? ""}
                  autoComplete="off"
                  className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                />
              </div>

              <div>
                <label className="mb-2 block text-sm">Secret Key</label>
                <input
                  type="password"
                  value={secretKey}
                  onChange={(event) => setSecretKey(event.target.value)}
                  placeholder={account?.hasSecretKey ? "Configured" : ""}
                  autoComplete="off"
                  className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                />
              </div>
            </div>

            <label className="flex items-center gap-3 border border-border px-4 py-3 text-sm">
              <input
                type="checkbox"
                checked={isEnabled}
                onChange={(event) => setIsEnabled(event.target.checked)}
                className="h-4 w-4"
              />
              Enable Iyzico account
            </label>

            <div className="flex flex-wrap justify-end gap-3 border-t border-border pt-6">
              {account ? (
                <button
                  type="button"
                  onClick={disableSettings}
                  disabled={isPending || !account.isEnabled}
                  className="inline-flex items-center gap-2 border border-border px-5 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
                >
                  <Power className="h-4 w-4" strokeWidth={1.5} />
                  Disable
                </button>
              ) : null}
              <button
                type="button"
                onClick={saveSettings}
                disabled={isPending || !canSave}
                className="inline-flex items-center gap-2 bg-primary px-6 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
              >
                <Save className="h-4 w-4" strokeWidth={1.5} />
                {isPending ? "Saving..." : "Save Settings"}
              </button>
            </div>
          </div>
        </section>

        <aside className="border border-border p-5">
          <dl className="space-y-4 text-sm">
            <div>
              <dt className="text-muted-foreground">Provider</dt>
              <dd className="mt-1">{formatEnumLabel(account?.provider ?? "Iyzico")}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Status</dt>
              <dd className="mt-1">{account ? formatEnumLabel(account.status) : "Not configured"}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Ready</dt>
              <dd className="mt-1">{account?.isReadyForPayments ? "Yes" : "No"}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">API Key</dt>
              <dd className="mt-1">{account?.apiKeyMasked ?? "-"}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Updated</dt>
              <dd className="mt-1">
                {account ? formatDateTime(account.updatedAtUtc) : "-"}
              </dd>
            </div>
          </dl>
        </aside>
      </div>
    </div>
  )
}
