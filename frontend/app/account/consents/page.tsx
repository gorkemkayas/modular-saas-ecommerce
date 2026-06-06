"use client"

import { useEffect, useMemo, useState } from "react"
import {
  Check,
  Info,
  Loader2,
  Mail,
  MessageSquare,
  UserCog,
  X,
  type LucideIcon,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { getMyProfile, upsertMyConsent } from "@/lib/api/account"
import { getApiErrorMessage } from "@/lib/api/error-message"
import type {
  ConsentType,
  CustomerConsentDto,
  UpdateConsentRequest,
} from "@/lib/api/types"

type PageState = "idle" | "saving" | "success" | "error"

interface ConsentDefinition {
  consentType: ConsentType
  title: string
  description: string
  icon: LucideIcon
}

interface ConsentViewModel {
  consentType: ConsentType
  title: string
  description: string
  icon: LucideIcon
  isGranted: boolean
  source: string
  updatedAtUtc: string | null
}

const consentDefinitions: ConsentDefinition[] = [
  {
    consentType: "EmailMarketing",
    title: "Email Marketing",
    description:
      "Receive promotional emails, exclusive offers, new arrivals, and style recommendations.",
    icon: Mail,
  },
  {
    consentType: "SmsMarketing",
    title: "SMS Marketing",
    description:
      "Get text messages about flash sales, order updates, and personalized deals.",
    icon: MessageSquare,
  },
  {
    consentType: "Profiling",
    title: "Profiling",
    description:
      "Allow us to analyze your preferences and shopping behavior to provide personalized experiences.",
    icon: UserCog,
  },
]

const consentSource = "AccountCenter"

function buildConsentViewModels(
  consents: CustomerConsentDto[],
): ConsentViewModel[] {
  const consentMap = new Map(consents.map((consent) => [consent.consentType, consent]))

  return consentDefinitions.map((definition) => {
    const storedConsent = consentMap.get(definition.consentType)

    return {
      ...definition,
      isGranted: storedConsent?.isGranted ?? false,
      source: storedConsent?.source ?? consentSource,
      updatedAtUtc: storedConsent?.updatedAtUtc ?? null,
    }
  })
}

function formatDate(dateString: string | null): string {
  if (!dateString) {
    return "Not updated yet"
  }

  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  })
}

export default function ConsentsPage() {
  const [consents, setConsents] = useState<ConsentViewModel[]>([])
  const [originalConsents, setOriginalConsents] = useState<ConsentViewModel[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [pageState, setPageState] = useState<PageState>("idle")
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const hasChanges = useMemo(
    () =>
      consents.some((consent, index) => {
        const original = originalConsents[index]
        return original ? consent.isGranted !== original.isGranted : false
      }),
    [consents, originalConsents],
  )

  useEffect(() => {
    let isMounted = true

    const loadConsents = async () => {
      try {
        setIsLoading(true)
        setErrorMessage(null)

        const profile = await getMyProfile()

        if (!isMounted) {
          return
        }

        const mappedConsents = buildConsentViewModels(profile.consents)
        setConsents(mappedConsents)
        setOriginalConsents(mappedConsents)
      } catch (error) {
        if (isMounted) {
          setErrorMessage(
            getApiErrorMessage(
              error,
              "Failed to load your consent preferences.",
            ),
          )
          setConsents(buildConsentViewModels([]))
          setOriginalConsents(buildConsentViewModels([]))
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    void loadConsents()

    return () => {
      isMounted = false
    }
  }, [])

  const toggleConsent = (consentType: ConsentType) => {
    setConsents((current) =>
      current.map((consent) =>
        consent.consentType === consentType
          ? { ...consent, isGranted: !consent.isGranted }
          : consent,
      ),
    )
    setPageState("idle")
    setErrorMessage(null)
  }

  const handleSave = async () => {
    if (!hasChanges) {
      return
    }

    try {
      setPageState("saving")
      setErrorMessage(null)

      const changedConsents = consents.filter((consent, index) => {
        const original = originalConsents[index]
        return original ? consent.isGranted !== original.isGranted : false
      })

      await Promise.all(
        changedConsents.map((consent) => {
          const request: UpdateConsentRequest = {
            isGranted: consent.isGranted,
            source: consentSource,
          }

          return upsertMyConsent(consent.consentType, request)
        }),
      )

      const refreshedProfile = await getMyProfile()
      const refreshedConsents = buildConsentViewModels(refreshedProfile.consents)
      setConsents(refreshedConsents)
      setOriginalConsents(refreshedConsents)
      setPageState("success")
    } catch (error) {
      setPageState("error")
      setErrorMessage(
        getApiErrorMessage(
          error,
          "Failed to save your consent preferences. Please try again.",
        ),
      )
    }
  }

  const handleReset = () => {
    setConsents(originalConsents)
    setPageState("idle")
    setErrorMessage(null)
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      <div>
        <h2 className="mb-4 text-xs uppercase tracking-[0.3em]">Consents</h2>
        <p className="text-muted-foreground">
          Manage your communication and data processing permissions. You can
          update these settings at any time.
        </p>
      </div>

      <div className="flex items-start gap-4 border border-border bg-secondary/30 p-6">
        <Info
          className="mt-0.5 h-5 w-5 shrink-0 text-muted-foreground"
          strokeWidth={1.5}
        />
        <p className="text-sm text-muted-foreground">
          You can update your communication and profiling permissions at any
          time. Changes will take effect immediately after saving.
        </p>
      </div>

      {pageState === "success" ? (
        <div className="flex items-center gap-3 border border-foreground/20 bg-foreground/5 p-4">
          <Check className="h-5 w-5 text-foreground" strokeWidth={1.5} />
          <p className="text-sm">
            Your consent preferences have been saved successfully.
          </p>
        </div>
      ) : null}

      {pageState === "error" || errorMessage ? (
        <div className="flex items-center gap-3 border border-destructive/30 bg-destructive/10 p-4">
          <X className="h-5 w-5 text-destructive" strokeWidth={1.5} />
          <p className="text-sm text-destructive">
            {errorMessage ?? "Failed to save your consent preferences. Please try again."}
          </p>
        </div>
      ) : null}

      <div className="space-y-4">
        {consents.map((consent) => {
          const Icon = consent.icon

          return (
            <div
              key={consent.consentType}
              className="border border-border p-6 transition-colors hover:bg-secondary/20"
            >
              <div className="flex items-start justify-between gap-6">
                <div className="flex min-w-0 flex-1 items-start gap-4">
                  <div className="flex h-10 w-10 shrink-0 items-center justify-center bg-secondary/50">
                    <Icon className="h-5 w-5" strokeWidth={1} />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="mb-2 flex flex-wrap items-center gap-3">
                      <h3 className="font-medium tracking-wide">{consent.title}</h3>
                      <span
                        className={`px-2 py-0.5 text-[10px] uppercase tracking-[0.15em] ${
                          consent.isGranted
                            ? "bg-foreground text-background"
                            : "bg-secondary text-muted-foreground"
                        }`}
                      >
                        {consent.isGranted ? "Granted" : "Revoked"}
                      </span>
                    </div>
                    <p className="mb-4 text-sm text-muted-foreground">
                      {consent.description}
                    </p>
                    <div className="flex flex-wrap gap-x-6 gap-y-1 text-xs text-muted-foreground">
                      <span>Source: {consent.source}</span>
                      <span>Last updated: {formatDate(consent.updatedAtUtc)}</span>
                    </div>
                  </div>
                </div>

                <button
                  type="button"
                  onClick={() => toggleConsent(consent.consentType)}
                  disabled={pageState === "saving"}
                  className={`relative h-7 w-14 shrink-0 transition-colors ${
                    consent.isGranted ? "bg-foreground" : "bg-border"
                  } ${pageState === "saving" ? "cursor-not-allowed opacity-50" : ""}`}
                  aria-label={`Toggle ${consent.title}`}
                  aria-pressed={consent.isGranted}
                >
                  <span
                    className={`absolute top-1 h-5 w-5 bg-background transition-transform ${
                      consent.isGranted ? "left-8" : "left-1"
                    }`}
                  />
                </button>
              </div>
            </div>
          )
        })}
      </div>

      <div className="flex items-center gap-4 border-t border-border pt-6">
        <Button
          onClick={handleSave}
          disabled={!hasChanges || pageState === "saving"}
          className="h-12 bg-foreground px-8 text-sm uppercase tracking-[0.2em] text-background hover:bg-foreground/90 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {pageState === "saving" ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Saving...
            </>
          ) : (
            "Save Changes"
          )}
        </Button>

        {hasChanges && pageState !== "saving" ? (
          <Button
            onClick={handleReset}
            variant="outline"
            className="h-12 border-border px-8 text-sm uppercase tracking-[0.2em] hover:bg-secondary"
          >
            Reset
          </Button>
        ) : null}
      </div>
    </div>
  )
}
