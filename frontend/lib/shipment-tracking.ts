const trackingNumberPlaceholders = [
  /\{\{\s*trackingNumber\s*\}\}/gi,
  /\{\s*trackingNumber\s*\}/gi,
  /\[\s*trackingNumber\s*\]/gi,
  /:trackingNumber/gi,
  /\{\{\s*tracking_number\s*\}\}/gi,
  /\{\s*tracking_number\s*\}/gi,
  /\[\s*tracking_number\s*\]/gi,
  /:tracking_number/gi,
  /tracking_number/gi,
  /tracking-number/gi,
  /trackingnumber/gi,
]

export function buildCarrierTrackingUrl(
  trackingUrlTemplate: string | null | undefined,
  trackingNumber: string | null | undefined,
): string | null {
  const template = trackingUrlTemplate?.trim()
  if (!template) {
    return null
  }

  const normalizedTrackingNumber = trackingNumber?.trim()
  const hasPlaceholder = trackingNumberPlaceholders.some((placeholder) => {
    placeholder.lastIndex = 0
    return placeholder.test(template)
  })

  if (!hasPlaceholder) {
    return template
  }

  if (!normalizedTrackingNumber) {
    return null
  }

  const encodedTrackingNumber = encodeURIComponent(normalizedTrackingNumber)
  return trackingNumberPlaceholders.reduce(
    (url, placeholder) => url.replace(placeholder, encodedTrackingNumber),
    template,
  )
}
