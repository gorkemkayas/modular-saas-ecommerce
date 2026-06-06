import { apiBaseUrl } from "@/lib/config"

const accessTokenCookieName = "ecommerce_access_token"

export class ApiError extends Error {
  status: number
  payload: unknown

  constructor(message: string, status: number, payload: unknown) {
    super(message)
    this.status = status
    this.payload = payload
  }
}

function buildUrl(path: string): string {
  if (path.startsWith("http://") || path.startsWith("https://")) {
    return path
  }

  return `${apiBaseUrl}${path}`
}

async function buildRequestUrl(path: string): Promise<string> {
  if (path === "/api/auth" || path.startsWith("/api/auth/")) {
    if (typeof window !== "undefined") {
      return path
    }

    try {
      const { headers } = await import("next/headers")
      const requestHeaders = await headers()
      const host =
        requestHeaders.get("x-forwarded-host") ?? requestHeaders.get("host")
      const protocol =
        requestHeaders.get("x-forwarded-proto") ??
        (host?.startsWith("localhost") ? "http" : "https")

      if (host) {
        return new URL(path, `${protocol}://${host}`).toString()
      }
    } catch {
      // Fall back to the configured API/base URL handling below.
    }
  }

  if (typeof window !== "undefined" && path.startsWith("/api/")) {
    return path
  }

  return buildUrl(path)
}

async function tryGetServerAccessToken(): Promise<string | null> {
  if (typeof window !== "undefined") {
    return null
  }

  try {
    const { cookies } = await import("next/headers")
    return (await cookies()).get(accessTokenCookieName)?.value ?? null
  } catch {
    return null
  }
}

export async function fetchJson<T>(
  path: string,
  init?: RequestInit,
): Promise<T> {
  const accessToken = await tryGetServerAccessToken()
  const requestUrl = await buildRequestUrl(path)

  const response = await fetch(requestUrl, {
    ...init,
    credentials: init?.credentials ?? "include",
    headers: {
      Accept: "application/json",
      ...(accessToken
        ? { Authorization: `Bearer ${accessToken}` }
        : {}),
      ...(init?.headers ?? {}),
    },
    cache: init?.cache ?? "no-store",
  })

  if (!response.ok) {
    let payload: unknown = null
    const errorText = await response.text()

    if (errorText) {
      const contentType = response.headers.get("content-type") ?? ""

      if (contentType.includes("application/json")) {
        try {
          payload = JSON.parse(errorText)
        } catch {
          payload = errorText
        }
      } else {
        payload = errorText
      }
    }

    throw new ApiError(
      `API request failed with status ${response.status}`,
      response.status,
      payload,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  const responseText = await response.text()

  if (!responseText) {
    return undefined as T
  }

  const contentType = response.headers.get("content-type") ?? ""

  if (contentType.includes("application/json")) {
    return JSON.parse(responseText) as T
  }

  return responseText as T
}

export async function postJson<TResponse, TRequest>(
  path: string,
  body: TRequest,
  init?: Omit<RequestInit, "body" | "method">,
): Promise<TResponse> {
  return fetchJson<TResponse>(path, {
    ...init,
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
    body: JSON.stringify(body),
  })
}

export async function postFormData<TResponse>(
  path: string,
  body: FormData,
  init?: Omit<RequestInit, "body" | "method">,
): Promise<TResponse> {
  return fetchJson<TResponse>(path, {
    ...init,
    method: "POST",
    body,
  })
}

export async function putJson<TResponse, TRequest>(
  path: string,
  body: TRequest,
  init?: Omit<RequestInit, "body" | "method">,
): Promise<TResponse> {
  return fetchJson<TResponse>(path, {
    ...init,
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
    body: JSON.stringify(body),
  })
}
