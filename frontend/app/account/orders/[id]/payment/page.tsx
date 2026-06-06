import Link from "next/link"
import { ArrowLeft, AlertCircle, Clock, CreditCard } from "lucide-react"
import { getOrderPayment } from "@/lib/api/account"
import { getAccountPath } from "@/lib/account-path"
import { formatDateTime, formatMoney, humanizeToken } from "@/lib/format"

export default async function OrderPaymentPage({
  params,
}: {
  params: Promise<{ id: string; storeSlug?: string }>
}) {
  const { id, storeSlug } = await params
  const accountPath = getAccountPath(storeSlug)
  const payment = await getOrderPayment(id)

  return (
    <div className="space-y-8">
      <Link
        href={`${accountPath}/orders/${id}`}
        className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <ArrowLeft className="h-4 w-4" strokeWidth={1} />
        Back to Order
      </Link>

      <div>
        <h2 className="text-xs tracking-[0.3em] uppercase">Payment Details</h2>
        <p className="text-sm text-muted-foreground mt-2">
          Payment history and provider feedback for order {payment.orderNumber}.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
        <section className="border border-border p-6">
          <div className="flex items-center gap-3 mb-4">
            <CreditCard className="h-5 w-5" strokeWidth={1} />
            <h3 className="text-xs tracking-[0.2em] uppercase">Amount</h3>
          </div>
          <p className="text-2xl font-medium tracking-wide">
            {formatMoney(payment.amount, payment.currencyCode)}
          </p>
        </section>

        <section className="border border-border p-6">
          <div className="flex items-center gap-3 mb-4">
            <Clock className="h-5 w-5" strokeWidth={1} />
            <h3 className="text-xs tracking-[0.2em] uppercase">Status</h3>
          </div>
          <p className="text-sm font-medium tracking-wide">{humanizeToken(payment.status)}</p>
        </section>

        <section className="border border-border p-6">
          <div className="flex items-center gap-3 mb-4">
            <AlertCircle className="h-5 w-5" strokeWidth={1} />
            <h3 className="text-xs tracking-[0.2em] uppercase">Method</h3>
          </div>
          <p className="text-sm font-medium tracking-wide">{humanizeToken(payment.methodType)}</p>
          <p className="text-sm text-muted-foreground mt-2">
            Provider: {humanizeToken(payment.provider)}
          </p>
        </section>
      </div>

      {payment.failureMessage ? (
        <section className="border border-destructive/30 bg-destructive/5 p-6 text-sm text-destructive">
          Failure reason: {payment.failureMessage}
        </section>
      ) : null}

      <section className="border border-border p-6">
        <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Payment Attempts</h3>
        {payment.attempts.length ? (
          <div className="space-y-4">
            {payment.attempts.map((attempt) => (
              <div key={attempt.id} className="border border-border p-4">
                <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-medium tracking-wide">
                      Attempt #{attempt.attemptNumber}
                    </p>
                    <p className="text-sm text-muted-foreground mt-1">
                      {humanizeToken(attempt.operationType)} - {humanizeToken(attempt.status)}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      Processed at {formatDateTime(attempt.processedAtUtc)}
                    </p>
                  </div>

                  <div className="text-sm text-muted-foreground sm:text-right">
                    <p>Idempotency: {attempt.idempotencyKey}</p>
                    {attempt.providerTransactionReference ? (
                      <p>Provider Ref: {attempt.providerTransactionReference}</p>
                    ) : null}
                  </div>
                </div>

                {attempt.failureMessage ? (
                  <p className="mt-4 text-sm text-destructive">
                    Failure: {attempt.failureMessage}
                  </p>
                ) : null}
              </div>
            ))}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">
            No payment attempts were returned for this order.
          </p>
        )}
      </section>

      <section className="border border-border p-6">
        <h3 className="text-xs tracking-[0.3em] uppercase mb-6">Refunds</h3>
        {payment.refunds.length ? (
          <div className="space-y-4">
            {payment.refunds.map((refund) => (
              <div key={refund.id} className="flex items-center justify-between border border-border p-4">
                <div>
                  <p className="font-medium tracking-wide">{humanizeToken(refund.reason)}</p>
                  <p className="text-sm text-muted-foreground mt-1">
                    {formatDateTime(refund.createdAtUtc)}
                  </p>
                </div>
                <p className="font-medium tracking-wide">
                  {formatMoney(refund.amount, payment.currencyCode)}
                </p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">No refunds recorded for this payment.</p>
        )}
      </section>
    </div>
  )
}
