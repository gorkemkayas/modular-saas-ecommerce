import { fetchJson, postJson, putJson } from "@/lib/api/client"
import type {
  CreateAddressRequest,
  ApiPagedResult,
  CustomerDto,
  OrderDto,
  OrderSummaryDto,
  PaymentActionResultDto,
  PaymentDto,
  ShippingCarrierDto,
  ShipmentDto,
  ShipmentSummaryDto,
  UpdateAddressRequest,
  UpdateConsentRequest,
  ConsentType,
} from "@/lib/api/types"
import { withQuery } from "@/lib/config"

export interface PlaceOrderItemInput {
  productId: string
  productVariantId: string | null
  quantity: number
}

export interface PlaceOrderRequest {
  shippingAddressId: string
  billingAddressId: string
  shippingCarrierId: string
  currencyCode: string
  items: PlaceOrderItemInput[]
}

export interface CreatePaymentRequest {
  methodType: string
}

export interface AuthorizePaymentRequest {
  idempotencyKey: string
}

export interface UpdateProfileRequest {
  firstName: string
  lastName: string
  phoneNumber: string | null
}

export interface UpdatePreferencesRequest {
  preferredLanguage: string | null
  preferredCurrency: string | null
}

export async function getMyProfile(): Promise<CustomerDto> {
  return fetchJson<CustomerDto>("/api/customers/me")
}

export async function getStorefrontShippingCarriers(
  storeSlug: string,
): Promise<ShippingCarrierDto[]> {
  return fetchJson<ShippingCarrierDto[]>(
    `/api/storefront/${encodeURIComponent(storeSlug)}/shipping-carriers`,
  )
}

export async function updateMyProfile(
  request: UpdateProfileRequest,
): Promise<void> {
  await putJson<void, UpdateProfileRequest>("/api/customers/me/profile", request)
}

export async function updateMyPreferences(
  request: UpdatePreferencesRequest,
): Promise<void> {
  await putJson<void, UpdatePreferencesRequest>(
    "/api/customers/me/preferences",
    request,
  )
}

export async function upsertMyConsent(
  consentType: ConsentType,
  request: UpdateConsentRequest,
): Promise<void> {
  await putJson<void, UpdateConsentRequest>(
    `/api/customers/me/consents/${consentType}`,
    request,
  )
}

export async function addMyAddress(
  request: CreateAddressRequest,
): Promise<{ addressId: string }> {
  return postJson<{ addressId: string }, CreateAddressRequest>(
    "/api/customers/me/addresses",
    request,
  )
}

export async function updateMyAddress(
  addressId: string,
  request: UpdateAddressRequest,
): Promise<void> {
  await putJson<void, UpdateAddressRequest>(
    `/api/customers/me/addresses/${addressId}`,
    request,
  )
}

export async function deleteMyAddress(addressId: string): Promise<void> {
  await fetchJson<void>(`/api/customers/me/addresses/${addressId}`, {
    method: "DELETE",
  })
}

export async function setDefaultShippingAddress(addressId: string): Promise<void> {
  await fetchJson<void>(
    `/api/customers/me/addresses/${addressId}/default-shipping`,
    {
      method: "POST",
    },
  )
}

export async function setDefaultBillingAddress(addressId: string): Promise<void> {
  await fetchJson<void>(
    `/api/customers/me/addresses/${addressId}/default-billing`,
    {
      method: "POST",
    },
  )
}

export async function getMyOrders(
  pageNumber = 1,
  pageSize = 20,
): Promise<ApiPagedResult<OrderSummaryDto>> {
  return fetchJson<ApiPagedResult<OrderSummaryDto>>(
    withQuery("/api/orders/me", { pageNumber, pageSize }),
  )
}

export async function getMyOrder(orderId: string): Promise<OrderDto> {
  return fetchJson<OrderDto>(`/api/orders/me/${orderId}`)
}

export async function getOrderPayment(orderId: string): Promise<PaymentDto> {
  return fetchJson<PaymentDto>(`/api/orders/${orderId}/payment`)
}

export async function getOrderShipments(
  orderId: string,
): Promise<ShipmentSummaryDto[]> {
  return fetchJson<ShipmentSummaryDto[]>(`/api/orders/me/${orderId}/shipments`)
}

export async function getOrderShipmentById(
  orderId: string,
  shipmentId: string,
): Promise<ShipmentDto> {
  return fetchJson<ShipmentDto>(
    `/api/orders/me/${orderId}/shipments/${shipmentId}`,
  )
}

export async function placeOrder(
  request: PlaceOrderRequest,
): Promise<{ orderId: string }> {
  return postJson<{ orderId: string }, PlaceOrderRequest>("/api/orders", request)
}

export async function createOrderPayment(
  orderId: string,
  request: CreatePaymentRequest,
): Promise<{ paymentId: string }> {
  return postJson<{ paymentId: string }, CreatePaymentRequest>(
    `/api/orders/${orderId}/payment`,
    request,
  )
}

export async function authorizeOrderPayment(
  orderId: string,
  request: AuthorizePaymentRequest,
): Promise<PaymentActionResultDto> {
  return postJson<PaymentActionResultDto, AuthorizePaymentRequest>(
    `/api/orders/${orderId}/payment/authorize`,
    request,
  )
}
