import { Check, Gauge, Lock, Sparkles } from "lucide-react"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { getApiErrorMessage } from "@/lib/api/error-message"
import {
  formatSubscriptionLimit,
  getCurrentSubscriptionOrNull,
  getPublicPlans,
  getSubscriptionFeatureLabel,
  getSubscriptionQuotaLabel,
  type SubscriptionPlanDto,
  type TenantSubscriptionDto,
} from "@/lib/api/subscription"

function PlanComparison({
  currentPlanCode,
  plans,
}: {
  currentPlanCode: string | null
  plans: SubscriptionPlanDto[]
}) {
  return (
    <div className="grid gap-4 lg:grid-cols-3">
      {plans.map((plan) => {
        const isCurrent = currentPlanCode === plan.code

        return (
          <section
            key={plan.code}
            className={`border p-5 ${
              isCurrent ? "border-foreground bg-secondary/35" : "border-border"
            }`}
          >
            <div className="flex items-start justify-between gap-4">
              <div>
                <h2 className="text-lg font-light">{plan.name}</h2>
                <p className="mt-1 text-sm text-muted-foreground">
                  {plan.description ?? "Subscription plan"}
                </p>
              </div>
              {isCurrent ? (
                <span className="bg-primary px-3 py-1 text-[10px] uppercase tracking-[0.2em] text-primary-foreground">
                  Current
                </span>
              ) : null}
            </div>

            <div className="mt-5 space-y-3">
              {plan.quotas.map((quota) => (
                <div
                  key={`${plan.code}-${quota.key}`}
                  className="flex items-center justify-between gap-4 text-sm"
                >
                  <span className="text-muted-foreground">
                    {getSubscriptionQuotaLabel(quota.key)}
                  </span>
                  <span>{formatSubscriptionLimit(quota.limit)}</span>
                </div>
              ))}
            </div>
          </section>
        )
      })}
    </div>
  )
}

function CurrentSubscriptionOverview({
  subscription,
}: {
  subscription: TenantSubscriptionDto
}) {
  return (
    <section className="border border-border p-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
            Current Plan
          </p>
          <h2 className="mt-2 text-2xl font-light tracking-wide">
            {subscription.planName}
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">
            {subscription.status} since{" "}
            {new Date(subscription.startedAtUtc).toLocaleDateString("en-US")}
          </p>
        </div>
        <span className="w-fit border border-border px-3 py-2 text-xs uppercase tracking-[0.2em] text-muted-foreground">
          {subscription.planCode}
        </span>
      </div>

      <div className="mt-6 grid gap-4 lg:grid-cols-2">
        <div className="border border-border/70 p-4">
          <div className="flex items-center gap-2 text-sm font-medium">
            <Gauge className="h-4 w-4" strokeWidth={1.5} />
            Quotas
          </div>
          <div className="mt-4 space-y-3">
            {subscription.quotas.map((quota) => (
              <div key={quota.key} className="flex items-center justify-between gap-4 text-sm">
                <span className="text-muted-foreground">
                  {getSubscriptionQuotaLabel(quota.key)}
                </span>
                <span>{formatSubscriptionLimit(quota.limit)}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="border border-border/70 p-4">
          <div className="flex items-center gap-2 text-sm font-medium">
            <Sparkles className="h-4 w-4" strokeWidth={1.5} />
            Features
          </div>
          <div className="mt-4 space-y-3">
            {subscription.features.map((feature) => {
              const Icon = feature.isEnabled ? Check : Lock

              return (
                <div key={feature.key} className="flex items-start gap-3 text-sm">
                  <Icon
                    className={`mt-0.5 h-4 w-4 ${
                      feature.isEnabled ? "text-foreground" : "text-muted-foreground"
                    }`}
                    strokeWidth={1.5}
                  />
                  <div>
                    <p>{getSubscriptionFeatureLabel(feature.key)}</p>
                    {feature.description ? (
                      <p className="mt-1 text-xs text-muted-foreground">
                        {feature.description}
                      </p>
                    ) : null}
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      </div>
    </section>
  )
}

export default async function AdminSubscriptionPage() {
  try {
    const [subscription, plans] = await Promise.all([
      getCurrentSubscriptionOrNull(),
      getPublicPlans(),
    ])

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Subscription</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Plan limits and feature access used by catalog, pricing, shipping, and storefront settings.
          </p>
        </div>

        {subscription ? (
          <CurrentSubscriptionOverview subscription={subscription} />
        ) : (
          <section className="border border-border bg-secondary/30 p-6">
            <h2 className="text-lg font-light">No subscription found</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              This tenant does not have a provisioned subscription record yet.
            </p>
          </section>
        )}

        <div className="space-y-4">
          <div>
            <h2 className="text-lg font-light tracking-wide">Available Plans</h2>
            <p className="mt-1 text-sm text-muted-foreground">
              These are read from the public plans endpoint.
            </p>
          </div>
          <PlanComparison currentPlanCode={subscription?.planCode ?? null} plans={plans} />
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Subscription could not be loaded"
        message={getApiErrorMessage(error, "The subscription request failed.")}
      />
    )
  }
}
