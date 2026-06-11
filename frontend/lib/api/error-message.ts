import { ApiError } from "@/lib/api/client"
import {
  getSubscriptionFeatureLabel,
  getSubscriptionQuotaLabel,
} from "@/lib/api/subscription"

type ProblemLikePayload = {
  detail?: unknown
  title?: unknown
  errors?: unknown
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null
}

function extractValidationMessage(errors: unknown): string | null {
  if (!isRecord(errors)) {
    return null
  }

  const messages = Object.values(errors)
    .flatMap((value) => (Array.isArray(value) ? value : []))
    .filter((value): value is string => typeof value === "string" && value.trim().length > 0)

  if (!messages.length) {
    return null
  }

  return messages.join(" ")
}

function extractString(payload: Record<string, unknown>, key: string): string | null {
  const value = payload[key]
  return typeof value === "string" && value.trim() ? value : null
}

function extractNumber(payload: Record<string, unknown>, key: string): number | null {
  const value = payload[key]
  return typeof value === "number" && Number.isFinite(value) ? value : null
}

function extractSubscriptionMessage(payload: Record<string, unknown>): string | null {
  const quotaKey = extractString(payload, "quotaKey")
  if (quotaKey) {
    const currentCount = extractNumber(payload, "currentCount")
    const limit = extractNumber(payload, "limit")
    const quotaLabel = getSubscriptionQuotaLabel(quotaKey)

    if (currentCount !== null && limit !== null) {
      return `${quotaLabel} limit reached for your current plan (${currentCount}/${limit}). Upgrade the plan or reduce usage before trying again.`
    }

    return `${quotaLabel} is limited by your current plan. Upgrade the plan or reduce usage before trying again.`
  }

  const featureKey = extractString(payload, "featureKey")
  if (featureKey) {
    return `${getSubscriptionFeatureLabel(featureKey)} is not available in your current plan. Upgrade the plan to use this feature.`
  }

  return null
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof ApiError)) {
    return fallback
  }

  if (error.status === 401) {
    return "Your session has expired. Please sign in again."
  }

  if (typeof error.payload === "string" && error.payload.trim()) {
    return error.payload
  }

  if (!isRecord(error.payload)) {
    return error.message || fallback
  }

  const payload = error.payload as ProblemLikePayload
  const subscriptionMessage = extractSubscriptionMessage(error.payload)

  if (subscriptionMessage) {
    return subscriptionMessage
  }

  if (error.status === 403) {
    return "You do not have permission to perform this action."
  }

  const validationMessage = extractValidationMessage(payload.errors)

  if (validationMessage) {
    return validationMessage
  }

  if (typeof payload.detail === "string" && payload.detail.trim()) {
    return payload.detail
  }

  if (typeof payload.title === "string" && payload.title.trim()) {
    return payload.title
  }

  return error.message || fallback
}
