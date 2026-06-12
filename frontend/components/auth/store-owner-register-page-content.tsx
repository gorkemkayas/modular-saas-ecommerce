"use client"

import { useState } from "react"
import Link from "next/link"
import {
  ArrowRight,
  Check,
  Eye,
  EyeOff,
  Lock,
  Sparkles,
  Store,
} from "lucide-react"

import { ApiError } from "@/lib/api/client"
import {
  formatSubscriptionLimit,
  getSubscriptionFeatureLabel,
  getSubscriptionQuotaLabel,
  subscriptionQuotaKeys,
  type SubscriptionPlanDto,
  type SubscriptionQuotaKey,
} from "@/lib/api/subscription"
import {
  registerStoreOwner,
  type RegisterStoreOwnerResponse,
} from "@/lib/api/store-owner-registration"
import { withQuery } from "@/lib/config"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import type { CSSProperties } from "react"

interface StoreOwnerRegisterPageContentProps {
  plans: SubscriptionPlanDto[]
  initialPlanCode?: string
  planLoadError?: string | null
}

type RegisterStep = "plan" | "planDetails" | "storeDetails"

const STORE_REGISTER_IMAGE_URL =
  "/images/platform/store-setup-owner-register.png"

const displayedQuotaKeys = [
  subscriptionQuotaKeys.catalogProducts,
  subscriptionQuotaKeys.catalogMediaPerProduct,
  subscriptionQuotaKeys.shippingCarriers,
] satisfies SubscriptionQuotaKey[]

const detailQuotaKeys = [
  subscriptionQuotaKeys.catalogProducts,
  subscriptionQuotaKeys.catalogCategories,
  subscriptionQuotaKeys.catalogMediaPerProduct,
  subscriptionQuotaKeys.pricingPriceLists,
  subscriptionQuotaKeys.shippingCarriers,
] satisfies SubscriptionQuotaKey[]

const stepOrder: RegisterStep[] = ["plan", "planDetails", "storeDetails"]

export function StoreOwnerRegisterPageContent({
  plans,
  initialPlanCode,
  planLoadError,
}: StoreOwnerRegisterPageContentProps) {
  const resolvedInitialPlanCode = plans.some((plan) => plan.code === initialPlanCode)
    ? initialPlanCode
    : ""
  const [step, setStep] = useState<RegisterStep>(
    resolvedInitialPlanCode ? "planDetails" : "plan",
  )
  const [selectedPlanCode, setSelectedPlanCode] = useState(
    resolvedInitialPlanCode,
  )
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(
    planLoadError ?? null,
  )
  const [registrationResult, setRegistrationResult] =
    useState<RegisterStoreOwnerResponse | null>(null)
  const [formData, setFormData] = useState({
    storeName: "",
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    acceptTerms: false,
  })

  const selectedPlan = plans.find((plan) => plan.code === selectedPlanCode)
  const currentStepIndex = stepOrder.indexOf(step)
  const canSubmit = Boolean(selectedPlanCode) && plans.length > 0 && !isLoading
  const stepSliderStyle = {
    "--step-offset": `-${currentStepIndex * 100}%`,
  } as CSSProperties

  const passwordRequirements = [
    { label: "At least 8 characters", met: formData.password.length >= 8 },
    { label: "Contains uppercase letter", met: /[A-Z]/.test(formData.password) },
    { label: "Contains number", met: /[0-9]/.test(formData.password) },
  ]

  function handlePlanSelect(planCode: string) {
    setSelectedPlanCode(planCode)
    setErrorMessage(null)
    setStep("planDetails")
  }

  function handleStoreDetailsStep() {
    if (!selectedPlan) {
      setErrorMessage("Choose a subscription plan before continuing.")
      setStep("plan")
      return
    }

    setErrorMessage(null)
    setStep("storeDetails")
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setErrorMessage(null)

    if (!selectedPlanCode || !selectedPlan) {
      setErrorMessage("Choose a subscription plan before creating the store.")
      setStep("plan")
      return
    }

    if (!formData.acceptTerms) {
      setErrorMessage("Please accept the terms before continuing.")
      return
    }

    setIsLoading(true)

    try {
      const result = await registerStoreOwner({
        name: formData.storeName.trim(),
        planCode: selectedPlanCode,
        owner: {
          name: formData.firstName.trim(),
          surname: formData.lastName.trim(),
          email: formData.email.trim(),
          password: formData.password,
        },
      })

      setRegistrationResult(result)
    } catch (error) {
      setErrorMessage(getStoreOwnerRegisterErrorMessage(error))
    } finally {
      setIsLoading(false)
    }
  }

  if (registrationResult) {
    const storeSlug = readString(registrationResult, "storeSlug")
    const loginHref = storeSlug
      ? withQuery("/auth/login", {
          storeSlug,
          intent: "admin",
          next: `/${storeSlug}/admin`,
        })
      : withQuery("/auth/login", { next: "/admin" })

    return (
      <main className="flex min-h-screen bg-background">
        <div className="relative hidden flex-1 lg:block">
          <div
            className="absolute inset-0 bg-cover bg-center"
            style={{ backgroundImage: `url('${STORE_REGISTER_IMAGE_URL}')` }}
          >
            <div className="absolute inset-0 bg-black/25" />
          </div>
        </div>

        <div className="flex flex-1 items-center justify-center px-4 py-10 sm:px-6 sm:py-12">
          <div className="w-full max-w-md">
            <div className="mb-10 sm:mb-12">
              <Link
                href="/"
                className="font-serif text-xl font-light tracking-[0.22em] sm:text-2xl sm:tracking-[0.3em]"
              >
                KAYAS
              </Link>
            </div>

            <div className="flex h-12 w-12 items-center justify-center rounded-md bg-primary text-primary-foreground">
              <Check className="h-5 w-5" strokeWidth={1.5} />
            </div>

            <div className="mt-8 mb-8">
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                Store Registration
              </p>
              <h1 className="mt-3 font-serif text-3xl lg:text-4xl font-light tracking-wide">
                Registration received
              </h1>
              <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
                AuthService will finish tenant creation and provision the store
                with the selected subscription plan.
              </p>
            </div>

            {registrationResult.requiresEmailVerification ? (
              <p className="mb-8 border border-border bg-secondary px-4 py-3 text-sm text-muted-foreground">
                Verify your email address before signing in to the admin panel.
              </p>
            ) : null}

            <div className="flex flex-col gap-3 sm:flex-row">
              <Button asChild className="h-12 flex-1">
                <Link href={loginHref}>
                  Admin Login
                  <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                </Link>
              </Button>
              <Button asChild variant="outline" className="h-12 flex-1">
                <Link href="/store-register">New Store</Link>
              </Button>
            </div>
          </div>
        </div>
      </main>
    )
  }

  return (
    <main className="flex min-h-screen bg-background">
      <div className="relative hidden flex-1 lg:block">
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{ backgroundImage: `url('${STORE_REGISTER_IMAGE_URL}')` }}
        >
          <div className="absolute inset-0 bg-black/20" />
          <div className="absolute bottom-10 left-10 max-w-sm text-white">
            <p className="text-xs uppercase tracking-[0.28em] text-white/70">
              Store Owner
            </p>
            <p className="mt-4 font-serif text-4xl font-light tracking-wide">
              Build a storefront on the same platform your customers shop.
            </p>
          </div>
        </div>
      </div>

      <div className="flex flex-1 items-center justify-center px-4 py-8 sm:px-6 sm:py-10">
        <div className="w-full max-w-2xl">
          <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between sm:gap-6">
            <Link
              href="/"
              className="font-serif text-xl font-light tracking-[0.22em] sm:text-2xl sm:tracking-[0.3em]"
            >
              KAYAS
            </Link>
            <Link
              href="/auth/login"
              className="text-sm text-muted-foreground underline underline-offset-4 transition-colors hover:text-foreground"
            >
              Sign in
            </Link>
          </div>

          <div className="mb-7 flex items-center gap-2 sm:gap-3">
            {stepOrder.map((item, index) => {
              const isCurrent = item === step
              const isCompleted = index < currentStepIndex

              return (
                <div key={item} className="flex flex-1 items-center gap-3">
                  <span
                    className={cn(
                      "flex h-8 w-8 shrink-0 items-center justify-center rounded-md border text-xs",
                      isCurrent
                        ? "border-foreground bg-primary text-primary-foreground"
                        : isCompleted
                          ? "border-border bg-secondary text-foreground"
                          : "border-border bg-background text-muted-foreground",
                    )}
                  >
                    {isCompleted ? (
                      <Check className="h-3.5 w-3.5" strokeWidth={1.5} />
                    ) : (
                      index + 1
                    )}
                  </span>
                  {index < stepOrder.length - 1 ? (
                    <div className="h-px flex-1 bg-border" />
                  ) : null}
                </div>
              )
            })}
          </div>

          <div className="overflow-visible md:min-h-[520px] md:overflow-hidden md:[height:min(720px,calc(100svh-9rem))]">
            <div
              className="block md:flex md:h-full md:transition-transform md:duration-500 md:ease-out md:[transform:translateX(var(--step-offset))]"
              style={stepSliderStyle}
            >
              <section
                className={cn(
                  "w-full shrink-0 overflow-visible pr-0 md:h-full md:overflow-y-auto md:pr-1",
                  step === "plan" ? "block" : "hidden md:block",
                )}
                aria-hidden={step !== "plan"}
              >
                <div className="mb-7">
                  <p className="flex items-center gap-2 text-xs uppercase tracking-[0.28em] text-muted-foreground">
                    <Sparkles className="h-4 w-4" strokeWidth={1.5} />
                    Package
                  </p>
                  <h1 className="mt-4 font-serif text-3xl lg:text-4xl font-light tracking-wide">
                    Choose your package
                  </h1>
                  <p className="mt-4 max-w-xl text-sm leading-relaxed text-muted-foreground">
                    Start with the limits and features that fit this store.
                    Details appear before the account form.
                  </p>
                </div>

                {errorMessage ? (
                  <div className="mb-6 border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {errorMessage}
                  </div>
                ) : null}

                {plans.length === 0 ? (
                  <div className="border border-border bg-secondary px-4 py-3 text-sm text-muted-foreground">
                    Subscription plans could not be loaded.
                  </div>
                ) : (
                  <div className="space-y-3">
                    {plans.map((plan) => {
                      const isSelected = selectedPlanCode === plan.code

                      return (
                        <button
                          key={plan.code}
                          type="button"
                          onClick={() => handlePlanSelect(plan.code)}
                          aria-pressed={isSelected}
                          className={cn(
                            "w-full border p-4 text-left transition-colors",
                            isSelected
                              ? "border-foreground bg-secondary"
                              : "border-border hover:bg-secondary/60",
                          )}
                        >
                          <div className="flex items-start justify-between gap-4">
                            <div>
                              <p className="text-base font-medium">{plan.name}</p>
                              <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                                {plan.description ?? "Subscription plan"}
                              </p>
                            </div>
                            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md border border-border">
                              {isSelected ? (
                                <Check className="h-4 w-4" strokeWidth={1.5} />
                              ) : (
                                <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                              )}
                            </span>
                          </div>

                          <div className="mt-4 grid gap-3 text-sm sm:grid-cols-3">
                            {displayedQuotaKeys.map((quotaKey) => (
                              <div
                                key={`${plan.code}-${quotaKey}`}
                                className="flex items-center justify-between gap-3 sm:block"
                              >
                                <span className="text-muted-foreground">
                                  {getSubscriptionQuotaLabel(quotaKey)}
                                </span>
                                <p className="sm:mt-1">
                                  {getPlanQuotaValue(plan, quotaKey)}
                                </p>
                              </div>
                            ))}
                          </div>
                        </button>
                      )
                    })}
                  </div>
                )}
              </section>

              <section
                className={cn(
                  "w-full shrink-0 overflow-visible px-0 md:h-full md:overflow-y-auto md:px-1",
                  step === "planDetails" ? "block" : "hidden md:block",
                )}
                aria-hidden={step !== "planDetails"}
              >
                <div className="mb-7">
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <p className="flex items-center gap-2 text-xs uppercase tracking-[0.28em] text-muted-foreground">
                      <Sparkles className="h-4 w-4" strokeWidth={1.5} />
                      Package Details
                    </p>
                    <button
                      type="button"
                      onClick={() => setStep("plan")}
                      className="text-sm text-muted-foreground underline underline-offset-4 transition-colors hover:text-foreground"
                    >
                      Change package
                    </button>
                  </div>
                  <h1 className="mt-4 font-serif text-3xl lg:text-4xl font-light tracking-wide">
                    Review your package
                  </h1>
                  <p className="mt-4 max-w-xl text-sm leading-relaxed text-muted-foreground">
                    These limits and features will be sent with the tenant
                    registration request.
                  </p>
                </div>

                {selectedPlan ? (
                  <div className="space-y-5">
                    <section className="border border-border bg-secondary px-5 py-5">
                      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                        <div>
                          <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                            Selected Package
                          </p>
                          <h2 className="mt-2 text-xl font-medium">
                            {selectedPlan.name}
                          </h2>
                          <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                            {selectedPlan.description ?? "Subscription plan"}
                          </p>
                        </div>
                        <span className="w-fit rounded-md bg-background px-3 py-2 text-xs uppercase tracking-[0.18em] text-muted-foreground">
                          {selectedPlan.code}
                        </span>
                      </div>
                    </section>

                    <section className="border border-border px-5 py-5">
                      <h3 className="text-xs uppercase tracking-[0.22em] text-muted-foreground">
                        Limits
                      </h3>
                      <div className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                        {detailQuotaKeys.map((quotaKey) => (
                          <div
                            key={quotaKey}
                            className="flex items-center justify-between gap-4 border-b border-border/70 pb-3 last:border-b-0 last:pb-0 sm:last:border-b sm:last:pb-3"
                          >
                            <span className="text-muted-foreground">
                              {getSubscriptionQuotaLabel(quotaKey)}
                            </span>
                            <span>{getPlanQuotaValue(selectedPlan, quotaKey)}</span>
                          </div>
                        ))}
                      </div>
                    </section>

                    <section className="border border-border px-5 py-5">
                      <h3 className="text-xs uppercase tracking-[0.22em] text-muted-foreground">
                        Features
                      </h3>
                      <div className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                        {selectedPlan.features.map((feature) => {
                          const Icon = feature.isEnabled ? Check : Lock

                          return (
                            <div key={feature.key} className="flex items-start gap-3">
                              <Icon
                                className={cn(
                                  "mt-0.5 h-4 w-4",
                                  feature.isEnabled
                                    ? "text-foreground"
                                    : "text-muted-foreground",
                                )}
                                strokeWidth={1.5}
                              />
                              <div>
                                <p>{getSubscriptionFeatureLabel(feature.key)}</p>
                                {feature.description ? (
                                  <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
                                    {feature.description}
                                  </p>
                                ) : null}
                              </div>
                            </div>
                          )
                        })}
                      </div>
                    </section>

                    <Button
                      type="button"
                      onClick={handleStoreDetailsStep}
                      className="h-14 w-full text-sm uppercase tracking-[0.2em]"
                    >
                      Continue
                      <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                    </Button>
                  </div>
                ) : (
                  <div className="border border-border bg-secondary px-4 py-3 text-sm text-muted-foreground">
                    Choose a package to review its details.
                  </div>
                )}
              </section>

              <section
                className={cn(
                  "w-full shrink-0 overflow-visible pl-0 md:h-full md:overflow-y-auto md:pl-1",
                  step === "storeDetails" ? "block" : "hidden md:block",
                )}
                aria-hidden={step !== "storeDetails"}
              >
                <div className="mb-7">
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <p className="flex items-center gap-2 text-xs uppercase tracking-[0.28em] text-muted-foreground">
                      <Store className="h-4 w-4" strokeWidth={1.5} />
                      Store Details
                    </p>
                    <button
                      type="button"
                      onClick={() => setStep("planDetails")}
                      className="text-sm text-muted-foreground underline underline-offset-4 transition-colors hover:text-foreground"
                    >
                      Back to package
                    </button>
                  </div>
                  <h1 className="mt-4 font-serif text-3xl lg:text-4xl font-light tracking-wide">
                    Create your store
                  </h1>
                  <p className="mt-4 max-w-xl text-sm leading-relaxed text-muted-foreground">
                    Enter the owner account details for the selected package.
                  </p>
                </div>

                {selectedPlan ? (
                  <div className="mb-5 flex flex-col gap-3 border border-border bg-secondary px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
                    <div>
                      <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
                        Selected Package
                      </p>
                      <p className="mt-1 text-sm font-medium">{selectedPlan.name}</p>
                    </div>
                    <span className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
                      {selectedPlan.code}
                    </span>
                  </div>
                ) : null}

                {errorMessage ? (
                  <div className="mb-6 border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {errorMessage}
                  </div>
                ) : null}

                <form onSubmit={handleSubmit} className="space-y-5">
                  <div>
                    <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                      Store Name
                    </label>
                    <Input
                      required
                      minLength={2}
                      value={formData.storeName}
                      onChange={(event) =>
                        setFormData({ ...formData, storeName: event.target.value })
                      }
                      className="h-14 bg-secondary border-0 text-sm tracking-wide"
                      placeholder="Demo Store"
                    />
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div>
                      <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                        First Name
                      </label>
                      <Input
                        required
                        value={formData.firstName}
                        onChange={(event) =>
                          setFormData({ ...formData, firstName: event.target.value })
                        }
                        className="h-14 bg-secondary border-0 text-sm tracking-wide"
                        placeholder="John"
                      />
                    </div>
                    <div>
                      <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                        Last Name
                      </label>
                      <Input
                        required
                        value={formData.lastName}
                        onChange={(event) =>
                          setFormData({ ...formData, lastName: event.target.value })
                        }
                        className="h-14 bg-secondary border-0 text-sm tracking-wide"
                        placeholder="Doe"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                      Email Address
                    </label>
                    <Input
                      type="email"
                      required
                      value={formData.email}
                      onChange={(event) =>
                        setFormData({ ...formData, email: event.target.value })
                      }
                      className="h-14 bg-secondary border-0 text-sm tracking-wide"
                      placeholder="owner@example.com"
                    />
                  </div>

                  <div>
                    <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                      Password
                    </label>
                    <div className="relative">
                      <Input
                        type={showPassword ? "text" : "password"}
                        required
                        minLength={8}
                        value={formData.password}
                        onChange={(event) =>
                          setFormData({ ...formData, password: event.target.value })
                        }
                        className="h-14 bg-secondary border-0 pr-12 text-sm tracking-wide"
                        placeholder="Create a secure password"
                      />
                      <button
                        type="button"
                        onClick={() => setShowPassword((current) => !current)}
                        className="absolute right-4 top-1/2 -translate-y-1/2 text-muted-foreground transition-colors hover:text-foreground"
                        aria-label={showPassword ? "Hide password" : "Show password"}
                      >
                        {showPassword ? (
                          <EyeOff className="h-5 w-5" strokeWidth={1} />
                        ) : (
                          <Eye className="h-5 w-5" strokeWidth={1} />
                        )}
                      </button>
                    </div>
                    {formData.password ? (
                      <div className="mt-3 grid gap-2 sm:grid-cols-3">
                        {passwordRequirements.map((requirement) => (
                          <div
                            key={requirement.label}
                            className={cn(
                              "flex items-center gap-2 text-xs",
                              requirement.met
                                ? "text-foreground"
                                : "text-muted-foreground",
                            )}
                          >
                            <Check
                              className={cn(
                                "h-3 w-3",
                                requirement.met ? "opacity-100" : "opacity-30",
                              )}
                              strokeWidth={1.5}
                            />
                            {requirement.label}
                          </div>
                        ))}
                      </div>
                    ) : null}
                  </div>

                  <label className="flex items-start gap-3">
                    <input
                      type="checkbox"
                      required
                      checked={formData.acceptTerms}
                      onChange={(event) =>
                        setFormData({
                          ...formData,
                          acceptTerms: event.target.checked,
                        })
                      }
                      className="mt-1 h-4 w-4 border-border accent-primary"
                    />
                    <span className="text-sm leading-relaxed text-muted-foreground">
                      I agree to the{" "}
                      <Link
                        href="/terms"
                        className="text-foreground underline underline-offset-4"
                      >
                        Terms of Service
                      </Link>{" "}
                      and{" "}
                      <Link
                        href="/privacy-policy"
                        className="text-foreground underline underline-offset-4"
                      >
                        Privacy Policy
                      </Link>
                    </span>
                  </label>

                  <Button
                    type="submit"
                    disabled={!canSubmit}
                    className="w-full h-14 bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase hover:bg-primary/90 transition-colors"
                  >
                    {isLoading ? "Creating store..." : "Create Store"}
                    {!isLoading ? (
                      <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                    ) : null}
                  </Button>
                </form>
              </section>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}

function getPlanQuotaValue(
  plan: SubscriptionPlanDto,
  quotaKey: SubscriptionQuotaKey,
): string {
  return formatSubscriptionLimit(
    plan.quotas.find((quota) => quota.key === quotaKey)?.limit,
  )
}

function getStoreOwnerRegisterErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (typeof error.payload === "string" && error.payload.trim()) {
      return error.payload
    }

    if (error.payload && typeof error.payload === "object") {
      const payload = error.payload as Record<string, unknown>

      if (typeof payload.detail === "string" && payload.detail.trim()) {
        return payload.detail
      }

      if (typeof payload.message === "string" && payload.message.trim()) {
        return payload.message
      }

      if (typeof payload.title === "string" && payload.title.trim()) {
        return payload.title
      }
    }
  }

  return "Store registration could not be completed right now."
}

function readString(
  payload: RegisterStoreOwnerResponse,
  key: "storeSlug",
): string | null {
  const value = payload[key]

  if (typeof value === "string" && value.trim()) {
    return value
  }

  if (payload.data && typeof payload.data === "object") {
    const data = payload.data as Record<string, unknown>
    const dataValue = data[key]

    if (typeof dataValue === "string" && dataValue.trim()) {
      return dataValue
    }
  }

  return null
}
