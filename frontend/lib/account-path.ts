import { storefrontPath } from "@/lib/config"

export function getAccountPath(storeSlug?: string | null, path = ""): string {
  const normalizedPath = path
    ? path.startsWith("/")
      ? path
      : `/${path}`
    : ""

  if (storeSlug) {
    return storefrontPath(storeSlug, `/account${normalizedPath}`)
  }

  return `/account${normalizedPath}`
}

export function resolveAccountBasePath(pathname: string): string {
  const storeScopedMatch = /^\/([^/]+)\/account(?:\/|$)/.exec(pathname)
  if (storeScopedMatch) {
    return `/${storeScopedMatch[1]}/account`
  }

  return "/account"
}
