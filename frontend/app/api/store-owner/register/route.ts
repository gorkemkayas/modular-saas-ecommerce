import { NextRequest, NextResponse } from "next/server"

const defaultRegisterPath = "/api/v1/tenants/register"

function getAuthServiceBaseUrl(): string {
  return (
    process.env.AUTH_SERVICE_BASE_URL ??
    process.env.NEXT_PUBLIC_AUTH_SERVICE_BASE_URL ??
    ""
  ).replace(/\/$/, "")
}

function getRegisterPath(): string {
  const configuredPath =
    process.env.AUTH_SERVICE_STORE_OWNER_REGISTER_PATH ??
    process.env.AUTH_SERVICE_TENANT_REGISTER_PATH ??
    defaultRegisterPath

  return configuredPath.startsWith("/")
    ? configuredPath
    : `/${configuredPath}`
}

function buildAuthServiceRegisterUrl(): string | null {
  const baseUrl = getAuthServiceBaseUrl()

  if (!baseUrl) {
    return null
  }

  return `${baseUrl}${getRegisterPath()}`
}

function problem(
  status: number,
  title: string,
  detail: string,
): NextResponse {
  return NextResponse.json({ status, title, detail }, { status })
}

export async function POST(request: NextRequest) {
  const authServiceUrl = buildAuthServiceRegisterUrl()

  if (!authServiceUrl) {
    return problem(
      503,
      "Store registration is not configured",
      "AUTH_SERVICE_BASE_URL must be configured for store-owner registration.",
    )
  }

  const headers = new Headers()
  const contentType = request.headers.get("content-type")
  const apiKey = process.env.AUTH_SERVICE_API_KEY?.trim()

  if (contentType) {
    headers.set("content-type", contentType)
  }

  headers.set("accept", "application/json")

  if (apiKey) {
    headers.set("authorization", `Bearer ${apiKey}`)
  }

  let authServiceResponse: Response

  try {
    authServiceResponse = await fetch(authServiceUrl, {
      method: "POST",
      headers,
      body: await request.text(),
      cache: "no-store",
    })
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unknown error"

    return problem(
      502,
      "Auth service could not be reached",
      message,
    )
  }

  const responseBody = await authServiceResponse.text()
  const allowsResponseBody = ![204, 205, 304].includes(
    authServiceResponse.status,
  )
  const responseHeaders = new Headers()
  const responseContentType = authServiceResponse.headers.get("content-type")
  const location = authServiceResponse.headers.get("location")

  if (responseContentType && allowsResponseBody) {
    responseHeaders.set("content-type", responseContentType)
  }

  if (location) {
    responseHeaders.set("location", location)
  }

  const response = new NextResponse(
    allowsResponseBody ? responseBody : null,
    {
      status: authServiceResponse.status,
      headers: responseHeaders,
    },
  )

  const setCookieHeaders =
    typeof authServiceResponse.headers.getSetCookie === "function"
      ? authServiceResponse.headers.getSetCookie()
      : []

  if (setCookieHeaders.length === 0) {
    const singleHeader = authServiceResponse.headers.get("set-cookie")

    if (singleHeader) {
      response.headers.append("set-cookie", singleHeader)
    }
  }

  for (const setCookieHeader of setCookieHeaders) {
    response.headers.append("set-cookie", setCookieHeader)
  }

  return response
}
