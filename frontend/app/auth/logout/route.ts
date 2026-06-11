import { NextResponse } from "next/server"

const accessTokenCookieName = "ecommerce_access_token"
const refreshTokenCookieName = "ecommerce_refresh_token"
const sessionPersistenceCookieName = "ecommerce_auth_persistent"

export async function GET(request: Request) {
  const url = new URL(request.url)
  const redirectTo = url.searchParams.get("redirectTo") || "/"
  const response = NextResponse.redirect(new URL(redirectTo, url.origin))

  response.cookies.delete(accessTokenCookieName)
  response.cookies.delete(refreshTokenCookieName)
  response.cookies.delete(sessionPersistenceCookieName)

  return response
}
