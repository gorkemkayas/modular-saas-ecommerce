import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminCustomerActions } from "@/components/admin/admin-customer-actions"
import { getCustomerById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

export default async function AdminCustomerDetailPage({ params }: { params: { id: string } }) {
  try {
    const customer = await getCustomerById(params.id)

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/customers" className="hover:text-foreground">
            Customers
          </Link>
          <span>/</span>
          <span className="text-foreground">
            {customer.firstName} {customer.lastName}
          </span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">
            {customer.firstName} {customer.lastName}
          </h1>
          <p className="mt-2 text-sm text-muted-foreground">
            {customer.email} • {formatEnumLabel(customer.status)}
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-3">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Phone</p>
            <p className="mt-2 text-sm">{customer.phoneNumber ?? "No phone number"}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Registered</p>
            <p className="mt-2 text-sm">{formatDateTime(customer.registeredAtUtc)}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Updated</p>
            <p className="mt-2 text-sm">{formatDateTime(customer.updatedAtUtc)}</p>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Preferences</h2>
          <div className="mt-4 grid gap-4 sm:grid-cols-2 text-sm">
            <div>
              <p className="text-xs uppercase tracking-wider text-muted-foreground">Language</p>
              <p className="mt-2">{customer.preferences.preferredLanguage ?? "Not set"}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wider text-muted-foreground">Currency</p>
              <p className="mt-2">{customer.preferences.preferredCurrency ?? "Not set"}</p>
            </div>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Addresses</h2>
          <div className="mt-4 space-y-4">
            {customer.addresses.length ? (
              customer.addresses.map((address) => (
                <div key={address.id} className="border border-border p-4 text-sm">
                  <p className="font-medium">{address.title}</p>
                  <p className="mt-1 text-muted-foreground">{address.contactName}</p>
                  <p className="text-muted-foreground">{address.line1}</p>
                  {address.line2 ? <p className="text-muted-foreground">{address.line2}</p> : null}
                  <p className="text-muted-foreground">
                    {address.district}, {address.city}, {address.country}
                  </p>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No saved addresses.</p>
            )}
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Consents</h2>
          <div className="mt-4 space-y-3">
            {customer.consents.length ? (
              customer.consents.map((consent) => (
                <div key={consent.consentType} className="flex items-center justify-between gap-4 border-b border-border pb-3 text-sm last:border-b-0 last:pb-0">
                  <div>
                    <p>{formatEnumLabel(consent.consentType)}</p>
                    <p className="text-xs text-muted-foreground">{consent.source}</p>
                  </div>
                  <span className="text-xs uppercase tracking-wider text-muted-foreground">
                    {consent.isGranted ? "Granted" : "Revoked"}
                  </span>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No consent records.</p>
            )}
          </div>
        </div>

        <AdminCustomerActions customerId={customer.id} status={customer.status} />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Customer detail could not be loaded"
        message={getApiErrorMessage(error, "The customer detail request failed.")}
      />
    )
  }
}
