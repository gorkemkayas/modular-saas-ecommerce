import { ApiError } from "@/lib/api/client"

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

export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof ApiError)) {
    return fallback
  }

  if (error.status === 401) {
    return "Your session has expired. Please sign in again."
  }

  if (error.status === 403) {
    return "You do not have permission to perform this action."
  }

  if (typeof error.payload === "string" && error.payload.trim()) {
    return error.payload
  }

  if (!isRecord(error.payload)) {
    return error.message || fallback
  }

  const payload = error.payload as ProblemLikePayload
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
