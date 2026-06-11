import { getMyProfile } from "@/lib/api/account"
import { PreferencesForm } from "@/components/account/preferences-form"

export default async function PreferencesPage() {
  const profile = await getMyProfile()

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-xs tracking-[0.3em] uppercase">Preferences & Profile</h2>
        <p className="text-sm text-muted-foreground mt-2">
          Update the profile and regional preference fields that are supported by the backend today.
        </p>
      </div>

      <PreferencesForm customer={profile} />
    </div>
  )
}
