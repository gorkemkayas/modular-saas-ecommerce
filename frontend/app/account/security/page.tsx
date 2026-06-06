import { Lock, Shield } from "lucide-react"
import { getMyProfile } from "@/lib/api/account"
import { formatDateTime, humanizeToken } from "@/lib/format"

export default async function SecurityPage() {
  const profile = await getMyProfile()

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-xs tracking-[0.3em] uppercase">Security</h2>
        <p className="text-sm text-muted-foreground mt-2">
          This backend exposes customer identity snapshots, but password and session management are handled by the external authentication service.
        </p>
      </div>

      <section className="border border-border p-6">
        <div className="flex items-center gap-3 mb-6">
          <Shield className="h-5 w-5" strokeWidth={1} />
          <h3 className="text-sm font-medium tracking-wide">Account Identity</h3>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 text-sm">
          <div>
            <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
              Account Status
            </p>
            <p className="font-medium tracking-wide">{humanizeToken(profile.status)}</p>
          </div>
          <div>
            <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
              External User Id
            </p>
            <p className="font-medium tracking-wide break-all">{profile.externalUserId}</p>
          </div>
          <div>
            <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
              Registered At
            </p>
            <p className="font-medium tracking-wide">{formatDateTime(profile.registeredAtUtc)}</p>
          </div>
          <div>
            <p className="text-xs tracking-[0.2em] text-muted-foreground uppercase mb-2">
              Last Updated
            </p>
            <p className="font-medium tracking-wide">{formatDateTime(profile.updatedAtUtc)}</p>
          </div>
        </div>
      </section>

      <section className="border border-border p-6">
        <div className="flex items-center gap-3 mb-6">
          <Lock className="h-5 w-5" strokeWidth={1} />
          <h3 className="text-sm font-medium tracking-wide">Authentication Flow</h3>
        </div>

        <div className="space-y-4 text-sm text-muted-foreground">
          <p>
            Password reset, session revocation, and multi-factor authentication should be managed in the external auth provider UI.
          </p>
          <p>
            This frontend keeps the security page aligned with the actual backend contract instead of simulating local password changes that are not supported here.
          </p>
        </div>
      </section>
    </div>
  )
}
