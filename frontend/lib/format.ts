export function formatMoney(amount: number, currencyCode: string): string {
  try {
    return new Intl.NumberFormat("tr-TR", {
      style: "currency",
      currency: currencyCode,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount)
  } catch {
    return `${amount.toFixed(2)} ${currencyCode}`
  }
}

export function formatDate(
  value: string | Date,
  locale = "tr-TR",
): string {
  const date = value instanceof Date ? value : new Date(value)
  return date.toLocaleDateString(locale, {
    year: "numeric",
    month: "long",
    day: "numeric",
  })
}

export function formatDateTime(
  value: string | Date,
  locale = "tr-TR",
): string {
  const date = value instanceof Date ? value : new Date(value)
  return date.toLocaleString(locale, {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  })
}

export function humanizeToken(value: string | null | undefined): string {
  if (!value) {
    return "-"
  }

  return value
    .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
    .replace(/[-_]+/g, " ")
    .trim()
    .replace(/\b\w/g, (char) => char.toUpperCase())
}
