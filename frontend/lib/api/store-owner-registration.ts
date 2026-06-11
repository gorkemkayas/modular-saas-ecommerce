import { postJson } from "@/lib/api/client"

export interface StoreOwnerRegistrationOwner {
  name: string
  surname: string
  email: string
  password: string
}

export interface RegisterStoreOwnerRequest {
  name: string
  planCode: string
  owner: StoreOwnerRegistrationOwner
}

export interface RegisterStoreOwnerResponse {
  tenantId?: number | string
  storeId?: string
  storeSlug?: string
  requiresEmailVerification?: boolean
  message?: string
  data?: unknown
}

export async function registerStoreOwner(
  request: RegisterStoreOwnerRequest,
): Promise<RegisterStoreOwnerResponse> {
  return postJson<RegisterStoreOwnerResponse, RegisterStoreOwnerRequest>(
    "/api/store-owner/register",
    request,
  )
}
