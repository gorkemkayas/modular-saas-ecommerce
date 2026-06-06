import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPaymentProviderSettingsManager } from "@/components/admin/admin-payment-provider-settings-manager"
import { getIyzicoPaymentProviderAccount } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export default async function PaymentSettingsPage() {
  try {
    const account = await getIyzicoPaymentProviderAccount()

    return <AdminPaymentProviderSettingsManager initialAccount={account} />
  } catch (error) {
    return (
      <AdminErrorState
        title="Payment settings could not be loaded"
        message={getApiErrorMessage(error, "The payment settings request failed.")}
      />
    )
  }
}
