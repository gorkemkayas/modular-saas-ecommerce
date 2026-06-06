import { cookies } from "next/headers"
import { NextRequest, NextResponse } from "next/server"

const accessTokenCookieName = "ecommerce_access_token"

function getBackendApiBaseUrl(): string {
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ?? ""

  if (!baseUrl) {
    throw new Error("NEXT_PUBLIC_API_BASE_URL must be configured for API proxy routes.")
  }

  return baseUrl
}

function buildBackendUrl(pathSegments: string[], request: NextRequest): string {
  const baseUrl = getBackendApiBaseUrl()
  const backendUrl = new URL(`${baseUrl}/api/${pathSegments.join("/")}`)

  request.nextUrl.searchParams.forEach((value, key) => {
    backendUrl.searchParams.append(key, value)
  })

  return backendUrl.toString()
}

async function proxyApiRequest(
  request: NextRequest,
  pathSegments: string[],
): Promise<NextResponse> {
  const backendUrl = buildBackendUrl(pathSegments, request)
  const requestCookies = await cookies()
  const accessToken = requestCookies.get(accessTokenCookieName)?.value
  const requestAuthorization = request.headers.get("authorization")

  const headers = new Headers()
  const contentType = request.headers.get("content-type")
  const accept = request.headers.get("accept")

  if (contentType) {
    headers.set("content-type", contentType)
  }

  if (accept) {
    headers.set("accept", accept)
  }

  if (requestAuthorization) {
    headers.set("authorization", requestAuthorization)
  } else if (accessToken) {
    headers.set("authorization", `Bearer ${accessToken}`)
  }

  const body =
    request.method === "GET" || request.method === "HEAD"
      ? undefined
      : await request.arrayBuffer()

  const backendResponse = await fetch(backendUrl, {
    method: request.method,
    headers,
    body,
    cache: "no-store",
  })

  const responseHeaders = new Headers()
  const responseContentType = backendResponse.headers.get("content-type")
  const allowsResponseBody =
    ![204, 205, 304].includes(backendResponse.status) && request.method !== "HEAD"

  if (responseContentType && allowsResponseBody) {
    responseHeaders.set("content-type", responseContentType)
  }

  const location = backendResponse.headers.get("location")
  if (location) {
    responseHeaders.set("location", location)
  }

  const responseBody = allowsResponseBody
    ? await backendResponse.arrayBuffer()
    : null

  return new NextResponse(responseBody, {
    status: backendResponse.status,
    headers: responseHeaders,
  })
}

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyApiRequest(request, path)
}

export async function POST(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyApiRequest(request, path)
}

export async function PUT(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyApiRequest(request, path)
}

export async function PATCH(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyApiRequest(request, path)
}

export async function DELETE(
  request: NextRequest,
  context: { params: Promise<{ path: string[] }> },
) {
  const { path } = await context.params
  return proxyApiRequest(request, path)
}
