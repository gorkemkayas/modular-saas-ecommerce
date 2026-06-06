export function normalizeOptionalInput(value: string): string | null {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

function normalizePhoneForValidation(value: string): string {
  return value
    .trim()
    .split("")
    .filter((character) => /\d/.test(character) || character === "+")
    .join("")
}

export function validateRequiredText(
  value: string,
  label: string,
  maxLength: number,
): string | null {
  const trimmed = value.trim()

  if (!trimmed) {
    return `${label} is required.`
  }

  if (trimmed.length > maxLength) {
    return `${label} must be ${maxLength} characters or fewer.`
  }

  return null
}

export function validateOptionalText(
  value: string,
  label: string,
  maxLength: number,
): string | null {
  const trimmed = value.trim()

  if (!trimmed) {
    return null
  }

  if (trimmed.length > maxLength) {
    return `${label} must be ${maxLength} characters or fewer.`
  }

  return null
}

export function validatePhoneNumber(
  value: string,
  options?: {
    label?: string
    required?: boolean
  },
): string | null {
  const label = options?.label ?? "Phone number"
  const required = options?.required ?? false
  const trimmed = value.trim()

  if (!trimmed) {
    return required ? `${label} is required.` : null
  }

  const normalized = normalizePhoneForValidation(trimmed)

  if (!normalized) {
    return `${label} format is invalid.`
  }

  const plusCount = normalized.split("").filter((character) => character === "+").length

  if (plusCount > 1 || (normalized.includes("+") && normalized[0] !== "+")) {
    return `${label} format is invalid.`
  }

  if (normalized.length < 7 || normalized.length > 20) {
    return `${label} must be between 7 and 20 characters after normalization.`
  }

  return null
}
