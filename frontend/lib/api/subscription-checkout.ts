import { postJson } from "@/lib/api/client"

export interface InitiateCheckoutRequest {
  tenantId: number
  planCode: string
  buyerEmail: string
  buyerName: string
  buyerPhone: string
  buyerIdentityNumber: string
}

export interface InitiateCheckoutResponse {
  subscriptionId: string
  paymentPageUrl: string
  token: string
}

export async function initiateSubscriptionCheckout(
  request: InitiateCheckoutRequest,
): Promise<InitiateCheckoutResponse> {
  return postJson<InitiateCheckoutResponse, InitiateCheckoutRequest>(
    "/api/subscription/checkout/initiate",
    request,
  )
}
