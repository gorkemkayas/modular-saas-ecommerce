export function getStoreDisplayName(
  storeName?: string | null,
  storeSlug?: string | null,
  fallback = "Storefront",
): string {
  const normalizedName = storeName?.trim()

  if (normalizedName) {
    return normalizedName
  }

  const normalizedSlug = storeSlug?.trim()

  if (normalizedSlug) {
    return normalizedSlug.replace(/[-_]+/g, " ").toUpperCase()
  }

  return fallback
}
