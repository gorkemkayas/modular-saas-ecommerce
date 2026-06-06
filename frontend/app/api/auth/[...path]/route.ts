import { cookies } from "next/headers"
import { NextRequest, NextResponse } from "next/server"

const accessTokenCookieName = "ecommerce_access_token"

function getBackendAuthBaseUrl(): string {
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ?? ""

  if (!baseUrl) {
    throw new Error("NEXT_PUBLIC_API_BASE_URL must be configured for auth proxy routes.")
  }

  return baseUrl
}

function buildBackendUrl(pathSegments: string[], request: NextRequest): string {
  const baseUrl = getBackendAuthBaseUrl()
  const backendUrl = new URL(`${baseUrl}/api/auth/${pathSegments.join("/")}`)

  request.nextUrl.searchParams.forEach((value, key) => {
    backendUrl.searchParams.append(key, value)
  })

  return backendUrl.toString()
}

async function proxyAuthRequest(
  request: NextRequest,
  pathSegments: string[],
): Promise<NextResponse> {
  const backendUrl = buildBackendUrl(pathSegments, request)
  const requestCookies = await cookies()
  const accessToken = requestCookies.get(accessTokenCookieName)?.value
  const requestAuthorization = request.headers.get("authorization")

  const headers = new Headers()
  const contentType = request.headers.get("content-type")

  if (contentType) {
    headers.set("content-type", contentType)
  }

  headers.set("accept", "application/json")

  if (requestAuthorization) {
    headers.set("authorization", requestAuthorization)
  } else if (accessToken) {
    headers.set("authorization", `Bearer ${accessToken}`)
  }

  const body =
    request.method === "GET" || request.method === "HEAD"
      ? undefined
      : await request.text()

  const backendResponse = await fetch(backendUrl, {
    method: request.method,
    headers,
    body,
    cache: "no-store",
  })

  const responseBody = await backendResponse.text()
  const responseHeaders = new Headers()
  const responseContentType = backendResponse.headers.get("content-type")
  const allowsResponseBody =
    ![204, 205, 304].includes(backendResponse.status) && request.method !== "HEAD"

  if (responseContentType && allowsResponseBody) {
    responseHeaders.set("content-type", responseContentType)
  }

  const response = new NextResponse(
    allowsResponseBody ? responseBody : null,
    {
      status: backendResponse.status,
      headers: responseHeaders,
    },
  )

  const setCookieHeaders =
    typeof backendResponse.headers.getSetCookie === "function"
      ? backendResponse.headers.getSetCookie()
      : []

  if (setCookieHeaders.length === 0) {
    const singleHeader = backendResponse.headers.get("set-cookie")
    if (singleHeader) {
      response.headers.append("set-cookie", singleHeader)
    }
  }

  for (const setCookieHeader of setCookieHeaders) {
    response.headers.append("set-cookie", setCookieHeader)
  }

  return response
}

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyAuthRequest(request, path)
}

export async function POST(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyAuthRequest(request, path)
}
