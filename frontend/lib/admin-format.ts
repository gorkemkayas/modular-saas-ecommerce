export function formatEnumLabel(value: string | null | undefined): string {
  if (!value) {
    return "Unknown"
  }

  return value
    .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
    .replace(/_/g, " ")
    .trim()
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return "Not available"
  }

  return new Intl.DateTimeFormat("tr-TR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value))
}

export function formatDate(value: string | null | undefined): string {
  if (!value) {
    return "Not available"
  }

  return new Intl.DateTimeFormat("tr-TR", {
    dateStyle: "medium",
  }).format(new Date(value))
}

export function formatMoney(amount: number, currencyCode: string): string {
  return new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency: currencyCode,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}
