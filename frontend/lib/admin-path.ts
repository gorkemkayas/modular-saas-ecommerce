export function getAdminPath(storeSlug?: string | null, path = ""): string {
  const normalizedPath = path
    ? path.startsWith("/")
      ? path
      : `/${path}`
    : ""

  if (storeSlug) {
    return `/${storeSlug}/admin${normalizedPath}`
  }

  return `/admin${normalizedPath}`
}

export function resolveAdminBasePath(pathname: string): string {
  const storeScopedMatch = /^\/([^/]+)\/admin(?:\/|$)/.exec(pathname)
  if (storeScopedMatch) {
    return `/${storeScopedMatch[1]}/admin`
  }

  return "/admin"
}

export function resolveAdminStoreSlug(pathname: string): string | null {
  const storeScopedMatch = /^\/([^/]+)\/admin(?:\/|$)/.exec(pathname)
  return storeScopedMatch?.[1] ?? null
}
