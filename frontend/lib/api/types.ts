export interface ApiPagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface StorefrontDto {
  tenantId: string
  name: string
  slug: string
  description: string | null
  logoUrl: string | null
  heroImageUrl: string | null
  heroMediaType: string | null
  heroEyebrowText: string | null
  heroTitle: string | null
  heroAccentTitle: string | null
  heroDescription: string | null
  heroPrimaryButtonText: string | null
  loginPageImageUrl: string | null
  registerPageImageUrl: string | null
}

export interface StorefrontResolvedPriceDto {
  productId: string
  productVariantId: string | null
  amount: number
  currencyCode: string
  compareAtAmount: number | null
  isOnSale: boolean
}

export interface StorefrontProductSummaryDto {
  id: string
  name: string
  slug: string
  shortDescription: string | null
  brandId: string | null
  brandName: string | null
  productType: string
  publishedAtUtc: string | null
  mainImageUrl: string | null
  price: StorefrontResolvedPriceDto | null
}

export interface StorefrontProductCategoryDto {
  categoryId: string
  name: string
  slug: string
}

export interface StorefrontProductAttributeDto {
  attributeDefinitionId: string
  name: string
  code: string
  value: string
  isVariantDefining: boolean
}

export interface StorefrontProductMediaDto {
  id: string
  productVariantId: string | null
  mediaType: string
  url: string
  altText: string | null
  isMain: boolean
  sortOrder: number
}

export interface StorefrontProductVariantDto {
  id: string
  name: string | null
  price: StorefrontResolvedPriceDto | null
  attributes: StorefrontProductAttributeDto[]
  mediaItems: StorefrontProductMediaDto[]
}

export interface StorefrontProductDto {
  id: string
  name: string
  shortDescription: string | null
  description: string | null
  slug: string
  brandId: string | null
  brandName: string | null
  productType: string
  publishedAtUtc: string | null
  price: StorefrontResolvedPriceDto | null
  categories: StorefrontProductCategoryDto[]
  attributes: StorefrontProductAttributeDto[]
  variants: StorefrontProductVariantDto[]
  mediaItems: StorefrontProductMediaDto[]
}

export interface StorefrontCategoryTreeNodeDto {
  id: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  parentCategoryId: string | null
  sortOrder: number
  children: StorefrontCategoryTreeNodeDto[]
}

export interface StorefrontBrandDto {
  id: string
  name: string
  slug: string
  description: string | null
  productCount: number
}

export interface StorefrontBrandFacetDto {
  brandId: string
  name: string
  count: number
}

export interface StorefrontFacetValueDto {
  value: string
  count: number
}

export interface StorefrontAttributeFacetDto {
  attributeDefinitionId: string
  name: string
  code: string
  values: StorefrontFacetValueDto[]
}

export interface StorefrontCatalogFacetsDto {
  brands: StorefrontBrandFacetDto[]
  attributes: StorefrontAttributeFacetDto[]
}

export type AddressType = "Home" | "Work" | "Other"

export interface CustomerAddressDto {
  id: string
  addressType: AddressType
  title: string
  contactName: string
  phoneNumber: string
  country: string
  city: string
  district: string
  line1: string
  line2: string | null
  postalCode: string | null
  isDefaultShipping: boolean
  isDefaultBilling: boolean
}

export interface CreateAddressRequest {
  addressType: AddressType
  title: string
  contactName: string
  phoneNumber: string
  country: string
  city: string
  district: string
  line1: string
  line2: string | null
  postalCode: string | null
  isDefaultShipping: boolean
  isDefaultBilling: boolean
}

export interface UpdateAddressRequest {
  addressType: AddressType
  title: string
  contactName: string
  phoneNumber: string
  country: string
  city: string
  district: string
  line1: string
  line2: string | null
  postalCode: string | null
}

export type ConsentType = "EmailMarketing" | "SmsMarketing" | "Profiling"

export interface CustomerConsentDto {
  consentType: ConsentType
  isGranted: boolean
  source: string
  updatedAtUtc: string
}

export interface UpdateConsentRequest {
  isGranted: boolean
  source: string
}

export interface CustomerPreferencesDto {
  preferredLanguage: string | null
  preferredCurrency: string | null
}

export interface CustomerDto {
  id: string
  tenantId: string
  externalUserId: string
  email: string
  firstName: string
  lastName: string
  phoneNumber: string | null
  status: string
  registeredAtUtc: string
  updatedAtUtc: string
  preferences: CustomerPreferencesDto
  addresses: CustomerAddressDto[]
  consents: CustomerConsentDto[]
}

export interface OrderSummaryDto {
  id: string
  orderNumber: string
  status: string
  paymentStatus: string
  fulfillmentStatus: string
  currencyCode: string
  shippingCarrierName: string | null
  itemCount: number
  grandTotalAmount: number
  placedAtUtc: string
}

export interface OrderAddressSnapshotDto {
  contactName: string
  phoneNumber: string
  country: string
  city: string
  district: string
  line1: string
  line2: string | null
  postalCode: string | null
}

export interface OrderCustomerSnapshotDto {
  email: string
  firstName: string
  lastName: string
  phoneNumber: string | null
}

export interface OrderPriceSnapshotDto {
  amount: number
  currencyCode: string
  compareAtAmount: number | null
}

export interface OrderItemDto {
  id: string
  productId: string
  productVariantId: string | null
  productName: string
  variantName: string | null
  sku: string | null
  quantity: number
  unitPrice: OrderPriceSnapshotDto
  lineTotalAmount: number
}

export interface OrderTotalsDto {
  subtotalAmount: number
  discountAmount: number
  shippingAmount: number
  taxAmount: number
  grandTotalAmount: number
}

export interface OrderShippingCarrierSnapshotDto {
  carrierId: string
  code: string
  name: string
  serviceCode: string | null
  serviceName: string | null
  trackingUrl: string | null
}

export interface OrderDto {
  id: string
  storeId: string
  customerId: string
  orderNumber: string
  status: string
  paymentStatus: string
  fulfillmentStatus: string
  currencyCode: string
  customer: OrderCustomerSnapshotDto
  billingAddress: OrderAddressSnapshotDto
  shippingAddress: OrderAddressSnapshotDto
  shippingCarrier: OrderShippingCarrierSnapshotDto | null
  totals: OrderTotalsDto
  placedAtUtc: string
  cancelledAtUtc: string | null
  cancellationReason: string | null
  reservationReference: string | null
  paymentReference: string | null
  shipmentReference: string | null
  createdAtUtc: string
  updatedAtUtc: string
  items: OrderItemDto[]
}

export interface PaymentAttemptDto {
  id: string
  attemptNumber: number
  operationType: string
  status: string
  idempotencyKey: string
  providerRequestReference: string | null
  providerTransactionReference: string | null
  failureCode: string | null
  failureMessage: string | null
  processedAtUtc: string
}

export interface PaymentRefundDto {
  id: string
  amount: number
  reason: string
  providerRefundReference: string | null
  createdAtUtc: string
}

export interface PaymentDto {
  id: string
  storeId: string
  orderId: string
  orderNumber: string
  customerId: string
  amount: number
  currencyCode: string
  status: string
  provider: string
  providerAccountId: string | null
  methodType: string
  externalPaymentReference: string | null
  externalConversationId: string | null
  failureCode: string | null
  failureMessage: string | null
  authorizedAtUtc: string | null
  capturedAtUtc: string | null
  cancelledAtUtc: string | null
  failedAtUtc: string | null
  refundedAmount: number
  createdAtUtc: string
  updatedAtUtc: string
  attempts: PaymentAttemptDto[]
  refunds: PaymentRefundDto[]
}

export interface PaymentActionResultDto {
  paymentId: string
  status: string
  externalPaymentReference: string | null
  externalConversationId: string | null
  actionUrl: string | null
  failureCode: string | null
  failureMessage: string | null
}

export interface ShipmentSummaryDto {
  id: string
  orderId: string
  orderNumber: string
  shipmentNumber: string
  status: string
  recipientName: string
  carrierName: string | null
  trackingNumber: string | null
  createdAtUtc: string
  shippedAtUtc: string | null
  deliveredAtUtc: string | null
}

export interface TrackingEventDto {
  id: string
  type: string
  occurredAtUtc: string
  location: string | null
  description: string
  rawStatusCode: string | null
  rawStatusText: string | null
}

export interface ShipmentPackageDto {
  id: string
  packageNumber: string
  trackingNumber: string | null
  weight: number | null
  weightUnit: string | null
  labelReference: string | null
  createdAtUtc: string
  shippedAtUtc: string | null
  trackingEvents: TrackingEventDto[]
}

export interface ShipmentLineDto {
  id: string
  orderItemId: string
  productId: string
  productVariantId: string | null
  productName: string
  variantName: string | null
  sku: string | null
  quantity: number
}

export interface ShipmentAddressDto {
  contactName: string
  phoneNumber: string
  country: string
  city: string
  district: string
  line1: string
  line2: string | null
  postalCode: string | null
}

export interface ShipmentDto {
  id: string
  storeId: string
  orderId: string
  orderNumber: string
  shipmentNumber: string
  status: string
  recipientName: string
  recipientPhoneNumber: string
  destinationAddress: ShipmentAddressDto
  carrierCode: string | null
  carrierName: string | null
  serviceCode: string | null
  serviceName: string | null
  trackingUrl: string | null
  internalNote: string | null
  cancellationReason: string | null
  createdAtUtc: string
  updatedAtUtc: string
  readyForDispatchAtUtc: string | null
  shippedAtUtc: string | null
  deliveredAtUtc: string | null
  cancelledAtUtc: string | null
  lines: ShipmentLineDto[]
  packages: ShipmentPackageDto[]
}

export interface ShippingCarrierDto {
  id: string
  storeId: string
  code: string
  name: string
  serviceCode: string | null
  serviceName: string | null
  trackingUrl: string | null
  isActive: boolean
  sortOrder: number
  createdAtUtc: string
  updatedAtUtc: string
}

export interface PriceEntryDto {
  id: string
  productId: string
  productVariantId: string | null
  amount: number
  compareAtAmount: number | null
  currencyCode: string
  isActive: boolean
}

export interface PriceListDto {
  id: string
  storeId: string
  name: string
  currencyCode: string
  priority: number
  isDefault: boolean
  status: string
  createdAtUtc: string
  updatedAtUtc: string
  entries: PriceEntryDto[]
}

export interface ProductSummaryDto {
  id: string
  storeId: string
  name: string
  slug: string
  brandId: string | null
  productType: string
  productStatus: string
  isPublished: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface ProductCategoryAssignmentDto {
  categoryId: string
  isPrimary: boolean
  sortOrder: number
}

export interface ProductAttributeValueDto {
  attributeDefinitionId: string
  productId: string | null
  productVariantId: string | null
  value: string
}

export interface ProductVariantDto {
  id: string
  productId: string
  sku: string
  name: string | null
  isActive: boolean
  sortOrder: number
  attributeValues: ProductAttributeValueDto[]
}

export interface ProductMediaDto {
  id: string
  productId: string
  productVariantId: string | null
  mediaType: string
  url: string
  altText: string | null
  isMain: boolean
  sortOrder: number
}

export interface AdminProductDto {
  id: string
  storeId: string
  name: string
  shortDescription: string | null
  description: string | null
  slug: string
  brandId: string | null
  sku: string | null
  productType: string
  productStatus: string
  isPublished: boolean
  publishedAtUtc: string | null
  createdAtUtc: string
  updatedAtUtc: string
  categories: ProductCategoryAssignmentDto[]
  attributeValues: ProductAttributeValueDto[]
  variants: ProductVariantDto[]
  mediaItems: ProductMediaDto[]
}

export interface CategoryDto {
  id: string
  storeId: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  parentCategoryId: string | null
  isActive: boolean
  sortOrder: number
  createdAtUtc: string
  updatedAtUtc: string
}

export interface CategoryTreeNodeDto {
  id: string
  storeId: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  parentCategoryId: string | null
  isActive: boolean
  sortOrder: number
  children: CategoryTreeNodeDto[]
}

export interface BrandDto {
  id: string
  storeId: string
  name: string
  slug: string
  description: string | null
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface AttributeDefinitionDto {
  id: string
  storeId: string
  name: string
  code: string
  dataType: string
  isRequired: boolean
  isFilterable: boolean
  isVariantDefining: boolean
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface InventoryReservationDto {
  id: string
  orderId: string
  reservationReference: string
  quantity: number
  status: string
  createdAtUtc: string
  releasedAtUtc: string | null
  confirmedAtUtc: string | null
}

export interface StockMovementDto {
  id: string
  type: string
  onHandDelta: number
  reservedDelta: number
  resultingOnHandQuantity: number
  resultingReservedQuantity: number
  reason: string
  reference: string | null
  createdAtUtc: string
}

export interface InventoryItemSummaryDto {
  id: string
  storeId: string
  productId: string
  productVariantId: string | null
  sku: string
  displayName: string
  onHandQuantity: number
  reservedQuantity: number
  availableQuantity: number
  reorderThreshold: number | null
  isLowStock: boolean
  updatedAtUtc: string
}

export interface InventoryItemDto {
  id: string
  storeId: string
  productId: string
  productVariantId: string | null
  sku: string
  displayName: string
  onHandQuantity: number
  reservedQuantity: number
  availableQuantity: number
  reorderThreshold: number | null
  isLowStock: boolean
  version: number
  createdAtUtc: string
  updatedAtUtc: string
  reservations: InventoryReservationDto[]
  recentMovements: StockMovementDto[]
}

export interface PriceListSummaryDto {
  id: string
  storeId: string
  name: string
  currencyCode: string
  priority: number
  isDefault: boolean
  status: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface PaymentSummaryDto {
  id: string
  orderId: string
  orderNumber: string
  amount: number
  currencyCode: string
  status: string
  provider: string
  providerAccountId: string | null
  methodType: string
  createdAtUtc: string
}

export interface IyzicoPaymentProviderAccountDto {
  id: string
  storeId: string
  provider: string
  status: string
  isEnabled: boolean
  isReadyForPayments: boolean
  apiKeyMasked: string | null
  hasSecretKey: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface CustomerSummaryDto {
  id: string
  externalUserId: string
  email: string
  fullName: string
  phoneNumber: string | null
  status: string
  addressCount: number
  registeredAtUtc: string
  updatedAtUtc: string
}

export interface NotificationTemplateSummaryDto {
  id: string
  trigger: string
  channel: string
  locale: string
  name: string
  isActive: boolean
  updatedAtUtc: string
}

export interface NotificationTemplateDto {
  id: string
  storeId: string
  trigger: string
  channel: string
  locale: string
  name: string
  subjectTemplate: string
  bodyTemplate: string
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export interface NotificationDispatchSummaryDto {
  id: string
  channel: string
  trigger: string
  status: string
  recipientAddress: string | null
  businessEntityType: string
  businessEntityId: string
  providerName: string | null
  lastProviderEventType: string | null
  createdAtUtc: string
  sentAtUtc: string | null
}

export interface NotificationAttemptDto {
  id: string
  attemptNumber: number
  status: string
  providerName: string
  providerRequestReference: string | null
  providerMessageId: string | null
  failureCode: string | null
  failureMessage: string | null
  attemptedAtUtc: string
}

export interface NotificationDispatchDto {
  id: string
  storeId: string
  channel: string
  trigger: string
  status: string
  recipientAddress: string | null
  recipientName: string | null
  subject: string | null
  body: string | null
  businessEntityType: string
  businessEntityId: string
  customerId: string | null
  providerName: string | null
  providerMessageId: string | null
  failureCode: string | null
  failureMessage: string | null
  suppressionReason: string | null
  lastProviderEventType: string | null
  createdAtUtc: string
  updatedAtUtc: string
  sentAtUtc: string | null
  lastAttemptAtUtc: string | null
  lastProviderEventAtUtc: string | null
  deliveredAtUtc: string | null
  openedAtUtc: string | null
  clickedAtUtc: string | null
  bouncedAtUtc: string | null
  complainedAtUtc: string | null
  attempts: NotificationAttemptDto[]
}

export interface StoreDto {
  id: string
  tenantId: string
  name: string
  slug: string
  description: string | null
  logoUrl: string | null
  status: string
  isPublished: boolean
  heroImageUrl: string | null
  heroMediaType: string | null
  heroEyebrowText: string | null
  heroTitle: string | null
  heroAccentTitle: string | null
  heroDescription: string | null
  heroPrimaryButtonText: string | null
  loginPageImageUrl: string | null
  registerPageImageUrl: string | null
}
