export const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ?? ""

export const defaultStoreSlug =
  process.env.NEXT_PUBLIC_DEFAULT_STORE_SLUG?.trim() || null

export function storefrontPath(storeSlug: string, path = ""): string {
  const normalizedPath = path.startsWith("/") ? path : path ? `/${path}` : ""
  return `/${storeSlug}${normalizedPath}`
}

export function withQuery(
  path: string,
  query: Record<string, string | number | boolean | null | undefined>,
): string {
  const search = new URLSearchParams()

  for (const [key, value] of Object.entries(query)) {
    if (value === undefined || value === null || value === "") {
      continue
    }

    search.set(key, String(value))
  }

  const queryString = search.toString()
  return queryString ? `${path}?${queryString}` : path
}
