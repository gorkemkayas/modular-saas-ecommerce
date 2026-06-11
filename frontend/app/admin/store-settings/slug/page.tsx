import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminStoreSettingsManager } from "@/components/admin/admin-store-settings-manager"
import { getStoreSettings } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { getCurrentSubscriptionOrNull } from "@/lib/api/subscription"

export default async function AdminStoreSettingsSlugPage() {
  try {
    const [store, subscription] = await Promise.all([
      getStoreSettings(),
      getCurrentSubscriptionOrNull(),
    ])

    return (
      <AdminStoreSettingsManager
        initialStore={store}
        initialSection="slug"
        subscription={subscription}
      />
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Store slug settings could not be loaded"
        message={getApiErrorMessage(error, "The store slug settings request failed.")}
      />
    )
  }
}
