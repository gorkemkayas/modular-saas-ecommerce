import Link from "next/link"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminPaymentActions } from "@/components/admin/admin-payment-actions"
import { getStorePaymentById } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel, formatMoney } from "@/lib/admin-format"

export default async function AdminPaymentDetailPage({ params }: { params: { id: string } }) {
  try {
    const payment = await getStorePaymentById(params.id)

    return (
      <div className="space-y-8">
        <nav className="flex items-center gap-2 text-sm text-muted-foreground">
          <Link href="/admin/payments" className="hover:text-foreground">
            Payments
          </Link>
          <span>/</span>
          <span className="text-foreground">{payment.id}</span>
        </nav>

        <div>
          <h1 className="text-3xl font-light tracking-tight">{payment.id}</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Order {payment.orderNumber} • {formatEnumLabel(payment.status)}
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-3">
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Amount</p>
            <p className="mt-2 text-2xl font-light">
              {formatMoney(payment.amount, payment.currencyCode)}
            </p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Provider</p>
            <p className="mt-2 text-sm">{formatEnumLabel(payment.provider)}</p>
          </div>
          <div className="border border-border p-6">
            <p className="text-xs uppercase tracking-wider text-muted-foreground">Method</p>
            <p className="mt-2 text-sm">{formatEnumLabel(payment.methodType)}</p>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Payment Metadata</h2>
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            <div>
              <p className="text-xs uppercase tracking-wider text-muted-foreground">External reference</p>
              <p className="mt-2 text-sm">{payment.externalPaymentReference ?? "Not set"}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wider text-muted-foreground">Conversation</p>
              <p className="mt-2 text-sm">{payment.externalConversationId ?? "Not set"}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wider text-muted-foreground">Authorized</p>
              <p className="mt-2 text-sm">{formatDateTime(payment.authorizedAtUtc)}</p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wider text-muted-foreground">Captured</p>
              <p className="mt-2 text-sm">{formatDateTime(payment.capturedAtUtc)}</p>
            </div>
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Attempts</h2>
          <div className="mt-4 space-y-4">
            {payment.attempts.length ? (
              payment.attempts.map((attempt) => (
                <div key={attempt.id} className="border border-border p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-sm font-medium">
                        Attempt {attempt.attemptNumber} • {formatEnumLabel(attempt.operationType)}
                      </p>
                      <p className="mt-1 text-xs text-muted-foreground">
                        {formatDateTime(attempt.processedAtUtc)}
                      </p>
                    </div>
                    <span className="text-xs uppercase tracking-wider text-muted-foreground">
                      {formatEnumLabel(attempt.status)}
                    </span>
                  </div>
                  {attempt.failureMessage ? (
                    <p className="mt-3 text-sm text-muted-foreground">{attempt.failureMessage}</p>
                  ) : null}
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No attempts recorded.</p>
            )}
          </div>
        </div>

        <div className="border border-border p-6">
          <h2 className="text-lg font-light tracking-wide">Refunds</h2>
          <div className="mt-4 space-y-4">
            {payment.refunds.length ? (
              payment.refunds.map((refund) => (
                <div key={refund.id} className="flex items-center justify-between gap-4 border-b border-border pb-3 text-sm last:border-b-0 last:pb-0">
                  <div>
                    <p>{formatMoney(refund.amount, payment.currencyCode)}</p>
                    <p className="text-xs text-muted-foreground">{refund.reason}</p>
                  </div>
                  <span className="text-xs text-muted-foreground">
                    {formatDateTime(refund.createdAtUtc)}
                  </span>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">No refunds recorded.</p>
            )}
          </div>
        </div>

        <AdminPaymentActions paymentId={payment.id} amount={payment.amount} />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Payment detail could not be loaded"
        message={getApiErrorMessage(error, "The payment detail request failed.")}
      />
    )
  }
}
