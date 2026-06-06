import { postJson } from "@/lib/api/client"
import { fetchJson } from "@/lib/api/client"

export interface RegisterCustomerRequest {
  storeSlug: string
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface RegisterCustomerResponse {
  tenantUserId: string
  customerId: string
  requiresEmailVerification: boolean
}

export interface LoginCustomerRequest {
  storeSlug: string
  email: string
  password: string
  isPersistent: boolean
  allowInactiveStore?: boolean
}

export interface AuthSessionResponse {
  isAuthenticated: boolean
  externalUserId: string | null
  email: string | null
  name: string | null
  tenantId: number | null
  canAccessAdmin: boolean
  storeSlug: string | null
}

export function isSessionForStore(
  session: Pick<AuthSessionResponse, "isAuthenticated" | "storeSlug">,
  storeSlug?: string | null,
): boolean {
  if (!session.isAuthenticated) {
    return false
  }

  if (!storeSlug) {
    return true
  }

  return session.storeSlug?.toLowerCase() === storeSlug.toLowerCase()
}

export async function registerCustomer(
  request: RegisterCustomerRequest,
): Promise<RegisterCustomerResponse> {
  return postJson<RegisterCustomerResponse, RegisterCustomerRequest>(
    "/api/auth/register",
    request,
  )
}

export async function loginCustomer(
  request: LoginCustomerRequest,
): Promise<void> {
  await postJson<void, LoginCustomerRequest>("/api/auth/login", request)
}

export async function getAuthSession(): Promise<AuthSessionResponse> {
  return fetchJson<AuthSessionResponse>("/api/auth/session")
}

export async function logoutCustomer(): Promise<void> {
  await postJson<void, Record<string, never>>("/api/auth/logout", {})
}
