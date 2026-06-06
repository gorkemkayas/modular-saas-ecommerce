import { ApiError, fetchJson, postFormData, postJson, putJson } from "@/lib/api/client"
import type {
  AdminProductDto,
  ApiPagedResult,
  AttributeDefinitionDto,
  BrandDto,
  CategoryDto,
  CategoryTreeNodeDto,
  CustomerDto,
  CustomerSummaryDto,
  InventoryItemDto,
  InventoryItemSummaryDto,
  IyzicoPaymentProviderAccountDto,
  NotificationDispatchDto,
  NotificationDispatchSummaryDto,
  NotificationTemplateDto,
  NotificationTemplateSummaryDto,
  PaymentDto,
  PaymentSummaryDto,
  PriceListDto,
  PriceListSummaryDto,
  ProductSummaryDto,
  ShippingCarrierDto,
  ShipmentDto,
  ShipmentSummaryDto,
  StockMovementDto,
  StoreDto,
} from "@/lib/api/types"
import { apiBaseUrl, withQuery } from "@/lib/config"

export type {
  AdminProductDto,
  AttributeDefinitionDto,
  BrandDto,
  CategoryTreeNodeDto,
  StoreDto,
} from "@/lib/api/types"

export interface ProductSearchOptions {
  searchTerm?: string
  status?: string
  productType?: string
  isPublished?: boolean
  categoryId?: string
  brandId?: string
  pageNumber?: number
  pageSize?: number
}

export interface InventorySearchOptions {
  productId?: string
  productVariantId?: string
  onlyLowStock?: boolean
  searchTerm?: string
  pageNumber?: number
  pageSize?: number
}

export interface PriceListSearchOptions {
  currencyCode?: string
  status?: string
  pageNumber?: number
  pageSize?: number
}

export interface PaymentSearchOptions {
  status?: string
  pageNumber?: number
  pageSize?: number
}

export interface ShipmentSearchOptions {
  status?: string
  orderId?: string
  orderNumber?: string
  shipmentNumber?: string
  trackingNumber?: string
  pageNumber?: number
  pageSize?: number
}

export interface CustomerSearchOptions {
  searchTerm?: string
  status?: string
  pageNumber?: number
  pageSize?: number
}

export interface NotificationTemplateSearchOptions {
  trigger?: string
  channel?: string
  isActive?: boolean
}

export interface NotificationDispatchSearchOptions {
  trigger?: string
  channel?: string
  status?: string
  businessEntityType?: string
  businessEntityId?: string
  pageNumber?: number
  pageSize?: number
}

export interface CreateSimpleProductRequest {
  name: string
  slug: string
  sku: string
  shortDescription: string | null
  description: string | null
  brandId: string | null
  categoryIds: string[]
}

export interface CreateVariantProductRequest {
  name: string
  slug: string
  shortDescription: string | null
  description: string | null
  brandId: string | null
  categoryIds: string[]
}

export interface UpdateProductDetailsRequest {
  name: string
  shortDescription: string | null
  description: string | null
  brandId: string | null
}

export interface ChangeProductSlugRequest {
  slug: string
}

export interface AssignProductCategoriesRequest {
  categoryIds: string[]
}

export interface ProductAttributeValueRequest {
  attributeDefinitionId: string
  value: string
}

export interface SetProductAttributesRequest {
  attributeValues: ProductAttributeValueRequest[]
}

export interface VariantAttributeValueRequest {
  attributeDefinitionId: string
  value: string
}

export interface AddVariantRequest {
  sku: string
  name: string | null
  sortOrder: number
  attributeValues: VariantAttributeValueRequest[]
}

export interface AddProductMediaRequest {
  mediaType: string
  url: string
  altText: string | null
  isMain: boolean
  sortOrder: number
  productVariantId: string | null
}

export interface UploadedProductMediaFileResponse {
  url: string
  mediaType: string
  originalFileName: string
}

export interface UploadedCategoryImageFileResponse {
  url: string
  originalFileName: string
}

export interface CreateInventoryItemRequest {
  productId: string
  productVariantId: string | null
  initialOnHandQuantity: number
  reorderThreshold: number | null
}

export interface AddStockRequest {
  quantity: number
  reason: string
  reference: string | null
}

export interface AdjustStockRequest {
  newOnHandQuantity: number
  reason: string
  reference: string | null
}

export interface SetReorderThresholdRequest {
  reorderThreshold: number | null
}

export interface CreatePriceListRequest {
  name: string
  currencyCode: string
  priority: number
  isDefault: boolean
}

export interface RenamePriceListRequest {
  name: string
}

export interface ChangePriceListPriorityRequest {
  priority: number
}

export interface SetProductPriceRequest {
  amount: number
  compareAtAmount: number | null
}

export interface SetVariantPriceRequest {
  amount: number
  compareAtAmount: number | null
}

export interface CapturePaymentRequest {
  idempotencyKey: string
}

export interface CancelPaymentRequest {
  idempotencyKey: string
}

export interface RefundPaymentRequest {
  amount: number
  reason: string
  idempotencyKey: string
}

export interface UpsertIyzicoPaymentProviderAccountRequest {
  apiKey: string | null
  secretKey: string | null
  isEnabled: boolean
}

export interface AddShipmentPackageRequest {
  trackingNumber: string | null
  weight: number | null
  weightUnit: string | null
  labelReference: string | null
}

export interface AssignShipmentCarrierRequest {
  carrierCode: string
  carrierName: string
  serviceCode: string | null
  serviceName: string | null
  trackingUrl: string | null
}

export interface ShippingCarrierRequest {
  code: string
  name: string
  serviceCode: string | null
  serviceName: string | null
  trackingUrl: string | null
  sortOrder: number
}

export interface UpdateShippingCarrierRequest extends ShippingCarrierRequest {
  isActive: boolean
}

export interface RegisterShipmentTrackingEventRequest {
  packageId: string
  type: string
  occurredAtUtc: string
  location: string | null
  description: string
  rawStatusCode: string | null
  rawStatusText: string | null
}

export interface CancelShipmentRequest {
  reason: string | null
}

export interface CreateNotificationTemplateRequest {
  trigger: string
  channel: string
  locale: string
  name: string
  subjectTemplate: string
  bodyTemplate: string
}

export interface UpdateNotificationTemplateRequest {
  locale: string
  name: string
  subjectTemplate: string
  bodyTemplate: string
}

export interface UpdateStoreProfileRequest {
  name: string
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

export interface UploadedStoreHeroMediaFileResponse {
  url: string
  mediaType: string
  originalFileName: string
}

export interface StoreSlugAvailabilityResponse {
  slug: string
  isAvailable: boolean
}

export interface StoreSlugSuggestionResponse {
  slug: string
}

export interface CreateBrandRequest {
  name: string
  slug: string
  description: string | null
}

export interface UpdateBrandRequest {
  name: string
  slug: string
  description: string | null
}

export interface CreateCategoryRequest {
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  parentCategoryId: string | null
  sortOrder: number
}

export interface UpdateCategoryRequest {
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  sortOrder: number
}

export interface ChangeCategoryParentRequest {
  parentCategoryId: string | null
}

export interface CreateAttributeDefinitionRequest {
  name: string
  code: string
  dataType: string
  isRequired: boolean
  isFilterable: boolean
  isVariantDefining: boolean
}

export interface UpdateAttributeDefinitionRequest {
  name: string
  code: string
  dataType: string
  isRequired: boolean
  isFilterable: boolean
  isVariantDefining: boolean
}

export async function searchProducts(
  options: ProductSearchOptions,
): Promise<ApiPagedResult<ProductSummaryDto>> {
  return fetchJson<ApiPagedResult<ProductSummaryDto>>(
    withQuery("/api/stores/me/products", {
      searchTerm: options.searchTerm,
      status: options.status,
      productType: options.productType,
      isPublished: options.isPublished,
      categoryId: options.categoryId,
      brandId: options.brandId,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getStoreSettings(): Promise<StoreDto> {
  return fetchJson<StoreDto>("/api/stores/me")
}

export async function listShippingCarriers(
  activeOnly = false,
): Promise<ShippingCarrierDto[]> {
  return fetchJson<ShippingCarrierDto[]>(
    withQuery("/api/stores/me/shipping-carriers", { activeOnly }),
  )
}

export async function createShippingCarrier(
  request: ShippingCarrierRequest,
): Promise<{ carrierId: string }> {
  return postJson<{ carrierId: string }, ShippingCarrierRequest>(
    "/api/stores/me/shipping-carriers",
    request,
  )
}

export async function updateShippingCarrier(
  carrierId: string,
  request: UpdateShippingCarrierRequest,
): Promise<void> {
  await putJson<void, UpdateShippingCarrierRequest>(
    `/api/stores/me/shipping-carriers/${carrierId}`,
    request,
  )
}

export async function updateStoreProfile(
  request: UpdateStoreProfileRequest,
): Promise<void> {
  await putJson<void, UpdateStoreProfileRequest>("/api/stores/profile", request)
}

export async function changeStoreSlug(newSlug: string): Promise<void> {
  await fetchJson<void>(withQuery("/api/stores/slug", { newSlug }), {
    method: "PUT",
  })
}

export async function publishStore(): Promise<void> {
  await fetchJson<void>("/api/stores/publish", {
    method: "POST",
  })
}

export async function unpublishStore(): Promise<void> {
  await fetchJson<void>("/api/stores/unpublish", {
    method: "POST",
  })
}

export async function checkStoreSlugAvailability(
  slug: string,
): Promise<StoreSlugAvailabilityResponse> {
  return fetchJson<StoreSlugAvailabilityResponse>(
    withQuery("/api/stores/slug-availability", { slug }),
  )
}

export async function suggestStoreSlug(
  slug: string,
): Promise<StoreSlugSuggestionResponse> {
  return fetchJson<StoreSlugSuggestionResponse>(
    withQuery("/api/stores/suggest-slug", { slug }),
  )
}

export async function getProductById(productId: string): Promise<AdminProductDto> {
  return fetchJson<AdminProductDto>(`/api/stores/me/products/${productId}`)
}

export async function getProductBySlug(slug: string): Promise<AdminProductDto> {
  return fetchJson<AdminProductDto>(`/api/stores/me/products/by-slug/${slug}`)
}

export async function createSimpleProduct(
  request: CreateSimpleProductRequest,
): Promise<void> {
  await postJson<void, CreateSimpleProductRequest>("/api/stores/me/products/simple", request)
}

export async function createVariantProduct(
  request: CreateVariantProductRequest,
): Promise<void> {
  await postJson<void, CreateVariantProductRequest>("/api/stores/me/products/variant", request)
}

export async function updateProductDetails(
  productId: string,
  request: UpdateProductDetailsRequest,
): Promise<void> {
  await putJson<void, UpdateProductDetailsRequest>(
    `/api/stores/me/products/${productId}`,
    request,
  )
}

export async function changeProductSlug(
  productId: string,
  request: ChangeProductSlugRequest,
): Promise<void> {
  await putJson<void, ChangeProductSlugRequest>(
    `/api/stores/me/products/${productId}/slug`,
    request,
  )
}

export async function assignProductCategories(
  productId: string,
  request: AssignProductCategoriesRequest,
): Promise<void> {
  await putJson<void, AssignProductCategoriesRequest>(
    `/api/stores/me/products/${productId}/categories`,
    request,
  )
}

export async function setProductAttributes(
  productId: string,
  request: SetProductAttributesRequest,
): Promise<void> {
  await putJson<void, SetProductAttributesRequest>(
    `/api/stores/me/products/${productId}/attributes`,
    request,
  )
}

export async function addVariant(
  productId: string,
  request: AddVariantRequest,
): Promise<void> {
  await postJson<void, AddVariantRequest>(
    `/api/stores/me/products/${productId}/variants`,
    request,
  )
}

export async function addProductMedia(
  productId: string,
  request: AddProductMediaRequest,
): Promise<void> {
  await postJson<void, AddProductMediaRequest>(
    `/api/stores/me/products/${productId}/media`,
    request,
  )
}

export async function uploadProductMediaFile(
  file: File,
  onProgress?: (progress: number) => void,
): Promise<UploadedProductMediaFileResponse> {
  if (typeof window !== "undefined") {
    return new Promise<UploadedProductMediaFileResponse>((resolve, reject) => {
      const formData = new FormData()
      formData.append("file", file)

      const xhr = new XMLHttpRequest()
      const requestUrl = "/api/stores/me/products/media/upload"

      xhr.open("POST", requestUrl)
      xhr.withCredentials = true
      xhr.setRequestHeader("Accept", "application/json")

      if (xhr.upload && onProgress) {
        xhr.upload.onprogress = (event) => {
          if (!event.lengthComputable) {
            return
          }

          onProgress(Math.round((event.loaded / event.total) * 100))
        }
      }

      xhr.onload = () => {
        const responseText = xhr.responseText
        let payload: unknown = null

        if (responseText) {
          try {
            payload = JSON.parse(responseText)
          } catch {
            payload = responseText
          }
        }

        if (xhr.status >= 200 && xhr.status < 300) {
          onProgress?.(100)
          resolve(payload as UploadedProductMediaFileResponse)
          return
        }

        reject(
          new ApiError(
            `API request failed with status ${xhr.status}`,
            xhr.status,
            payload,
          ),
        )
      }

      xhr.onerror = () => {
        reject(new Error("The media upload request failed."))
      }

      xhr.send(formData)
    })
  }

  const formData = new FormData()
  formData.append("file", file)

  return postFormData<UploadedProductMediaFileResponse>(
    "/api/stores/me/products/media/upload",
    formData,
  )
}

export async function uploadCategoryImageFile(
  file: File,
  onProgress?: (progress: number) => void,
): Promise<UploadedCategoryImageFileResponse> {
  if (typeof window !== "undefined") {
    return new Promise<UploadedCategoryImageFileResponse>((resolve, reject) => {
      const formData = new FormData()
      formData.append("file", file)

      const xhr = new XMLHttpRequest()
      const requestUrl = "/api/stores/me/categories/image/upload"

      xhr.open("POST", requestUrl)
      xhr.withCredentials = true
      xhr.setRequestHeader("Accept", "application/json")

      if (xhr.upload && onProgress) {
        xhr.upload.onprogress = (event) => {
          if (!event.lengthComputable) {
            return
          }

          onProgress(Math.round((event.loaded / event.total) * 100))
        }
      }

      xhr.onload = () => {
        const responseText = xhr.responseText
        let payload: unknown = null

        if (responseText) {
          try {
            payload = JSON.parse(responseText)
          } catch {
            payload = responseText
          }
        }

        if (xhr.status >= 200 && xhr.status < 300) {
          onProgress?.(100)
          resolve(payload as UploadedCategoryImageFileResponse)
          return
        }

        reject(
          new ApiError(
            `API request failed with status ${xhr.status}`,
            xhr.status,
            payload,
          ),
        )
      }

      xhr.onerror = () => {
        reject(new Error("The category image upload request failed."))
      }

      xhr.send(formData)
    })
  }

  const formData = new FormData()
  formData.append("file", file)

  return postFormData<UploadedCategoryImageFileResponse>(
    "/api/stores/me/categories/image/upload",
    formData,
  )
}

export async function uploadStoreHeroMediaFile(
  file: File,
  onProgress?: (progress: number) => void,
): Promise<UploadedStoreHeroMediaFileResponse> {
  if (typeof window !== "undefined") {
    return new Promise<UploadedStoreHeroMediaFileResponse>((resolve, reject) => {
      const formData = new FormData()
      formData.append("file", file)

      const xhr = new XMLHttpRequest()
      const requestUrl = "/api/stores/hero-media/upload"

      xhr.open("POST", requestUrl)
      xhr.withCredentials = true
      xhr.setRequestHeader("Accept", "application/json")

      if (xhr.upload && onProgress) {
        xhr.upload.onprogress = (event) => {
          if (!event.lengthComputable) {
            return
          }

          onProgress(Math.round((event.loaded / event.total) * 100))
        }
      }

      xhr.onload = () => {
        const responseText = xhr.responseText
        let payload: unknown = null

        if (responseText) {
          try {
            payload = JSON.parse(responseText)
          } catch {
            payload = responseText
          }
        }

        if (xhr.status >= 200 && xhr.status < 300) {
          onProgress?.(100)
          resolve(payload as UploadedStoreHeroMediaFileResponse)
          return
        }

        reject(
          new ApiError(
            `API request failed with status ${xhr.status}`,
            xhr.status,
            payload,
          ),
        )
      }

      xhr.onerror = () => {
        reject(new Error("The store hero media upload request failed."))
      }

      xhr.send(formData)
    })
  }

  const formData = new FormData()
  formData.append("file", file)

  return postFormData<UploadedStoreHeroMediaFileResponse>(
    "/api/stores/hero-media/upload",
    formData,
  )
}

export async function activateProduct(productId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/products/${productId}/activate`, {
    method: "POST",
  })
}

export async function publishProduct(productId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/products/${productId}/publish`, {
    method: "POST",
  })
}

export async function unpublishProduct(productId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/products/${productId}/unpublish`, {
    method: "POST",
  })
}

export async function archiveProduct(productId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/products/${productId}/archive`, {
    method: "POST",
  })
}

export async function getCategoryTree(): Promise<CategoryTreeNodeDto[]> {
  return fetchJson<CategoryTreeNodeDto[]>("/api/stores/me/categories/tree")
}

export async function getCategoryById(categoryId: string): Promise<CategoryDto> {
  return fetchJson<CategoryDto>(`/api/stores/me/categories/${categoryId}`)
}

export async function createCategory(
  request: CreateCategoryRequest,
): Promise<void> {
  await postJson<void, CreateCategoryRequest>("/api/stores/me/categories", request)
}

export async function updateCategory(
  categoryId: string,
  request: UpdateCategoryRequest,
): Promise<void> {
  await putJson<void, UpdateCategoryRequest>(
    `/api/stores/me/categories/${categoryId}`,
    request,
  )
}

export async function changeCategoryParent(
  categoryId: string,
  request: ChangeCategoryParentRequest,
): Promise<void> {
  await putJson<void, ChangeCategoryParentRequest>(
    `/api/stores/me/categories/${categoryId}/parent`,
    request,
  )
}

export async function activateCategory(categoryId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/categories/${categoryId}/activate`, {
    method: "POST",
  })
}

export async function deactivateCategory(categoryId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/categories/${categoryId}/deactivate`, {
    method: "POST",
  })
}

export async function searchBrands(searchTerm?: string): Promise<BrandDto[]> {
  return fetchJson<BrandDto[]>(
    withQuery("/api/stores/me/brands", { searchTerm, activeOnly: false }),
  )
}

export async function getBrandById(brandId: string): Promise<BrandDto> {
  return fetchJson<BrandDto>(`/api/stores/me/brands/${brandId}`)
}

export async function createBrand(request: CreateBrandRequest): Promise<void> {
  await postJson<void, CreateBrandRequest>("/api/stores/me/brands", request)
}

export async function updateBrand(
  brandId: string,
  request: UpdateBrandRequest,
): Promise<void> {
  await putJson<void, UpdateBrandRequest>(
    `/api/stores/me/brands/${brandId}`,
    request,
  )
}

export async function activateBrand(brandId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/brands/${brandId}/activate`, {
    method: "POST",
  })
}

export async function deactivateBrand(brandId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/brands/${brandId}/deactivate`, {
    method: "POST",
  })
}

export async function listAttributeDefinitions(
  activeOnly = false,
): Promise<AttributeDefinitionDto[]> {
  return fetchJson<AttributeDefinitionDto[]>(
    withQuery("/api/stores/me/attributes", { activeOnly }),
  )
}

export async function getAttributeDefinitionById(
  attributeDefinitionId: string,
): Promise<AttributeDefinitionDto> {
  return fetchJson<AttributeDefinitionDto>(
    `/api/stores/me/attributes/${attributeDefinitionId}`,
  )
}

export async function createAttributeDefinition(
  request: CreateAttributeDefinitionRequest,
): Promise<void> {
  await postJson<void, CreateAttributeDefinitionRequest>(
    "/api/stores/me/attributes",
    request,
  )
}

export async function updateAttributeDefinition(
  attributeDefinitionId: string,
  request: UpdateAttributeDefinitionRequest,
): Promise<void> {
  await putJson<void, UpdateAttributeDefinitionRequest>(
    `/api/stores/me/attributes/${attributeDefinitionId}`,
    request,
  )
}

export async function activateAttributeDefinition(
  attributeDefinitionId: string,
): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/attributes/${attributeDefinitionId}/activate`,
    {
      method: "POST",
    },
  )
}

export async function deactivateAttributeDefinition(
  attributeDefinitionId: string,
): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/attributes/${attributeDefinitionId}/deactivate`,
    {
      method: "POST",
    },
  )
}

export async function searchInventoryItems(
  options: InventorySearchOptions,
): Promise<ApiPagedResult<InventoryItemSummaryDto>> {
  return fetchJson<ApiPagedResult<InventoryItemSummaryDto>>(
    withQuery("/api/stores/me/inventory/items", {
      productId: options.productId,
      productVariantId: options.productVariantId,
      onlyLowStock: options.onlyLowStock ?? false,
      searchTerm: options.searchTerm,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getInventoryItemById(
  inventoryItemId: string,
): Promise<InventoryItemDto> {
  return fetchJson<InventoryItemDto>(
    `/api/stores/me/inventory/items/${inventoryItemId}`,
  )
}

export async function createInventoryItem(
  request: CreateInventoryItemRequest,
): Promise<void> {
  await postJson<void, CreateInventoryItemRequest>(
    "/api/stores/me/inventory/items",
    request,
  )
}

export async function addStockToInventoryItem(
  inventoryItemId: string,
  request: AddStockRequest,
): Promise<void> {
  await postJson<void, AddStockRequest>(
    `/api/stores/me/inventory/items/${inventoryItemId}/stock/add`,
    request,
  )
}

export async function adjustInventoryItemStock(
  inventoryItemId: string,
  request: AdjustStockRequest,
): Promise<void> {
  await putJson<void, AdjustStockRequest>(
    `/api/stores/me/inventory/items/${inventoryItemId}/stock/adjust`,
    request,
  )
}

export async function setInventoryReorderThreshold(
  inventoryItemId: string,
  request: SetReorderThresholdRequest,
): Promise<void> {
  await putJson<void, SetReorderThresholdRequest>(
    `/api/stores/me/inventory/items/${inventoryItemId}/reorder-threshold`,
    request,
  )
}

export async function getInventoryMovements(
  inventoryItemId: string,
  pageNumber = 1,
  pageSize = 50,
): Promise<ApiPagedResult<StockMovementDto>> {
  return fetchJson<ApiPagedResult<StockMovementDto>>(
    withQuery(`/api/stores/me/inventory/items/${inventoryItemId}/movements`, {
      pageNumber,
      pageSize,
    }),
  )
}

export async function searchPriceLists(
  options: PriceListSearchOptions,
): Promise<ApiPagedResult<PriceListSummaryDto>> {
  return fetchJson<ApiPagedResult<PriceListSummaryDto>>(
    withQuery("/api/stores/me/pricing/lists", {
      currencyCode: options.currencyCode,
      status: options.status,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getPriceListById(priceListId: string): Promise<PriceListDto> {
  return fetchJson<PriceListDto>(`/api/stores/me/pricing/lists/${priceListId}`)
}

export async function createPriceList(
  request: CreatePriceListRequest,
): Promise<void> {
  await postJson<void, CreatePriceListRequest>(
    "/api/stores/me/pricing/lists",
    request,
  )
}

export async function renamePriceList(
  priceListId: string,
  request: RenamePriceListRequest,
): Promise<void> {
  await putJson<void, RenamePriceListRequest>(
    `/api/stores/me/pricing/lists/${priceListId}/name`,
    request,
  )
}

export async function changePriceListPriority(
  priceListId: string,
  request: ChangePriceListPriorityRequest,
): Promise<void> {
  await putJson<void, ChangePriceListPriorityRequest>(
    `/api/stores/me/pricing/lists/${priceListId}/priority`,
    request,
  )
}

export async function setDefaultPriceList(priceListId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/pricing/lists/${priceListId}/default`, {
    method: "POST",
  })
}

export async function activatePriceList(priceListId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/pricing/lists/${priceListId}/activate`, {
    method: "POST",
  })
}

export async function deactivatePriceList(priceListId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/pricing/lists/${priceListId}/deactivate`, {
    method: "POST",
  })
}

export async function archivePriceList(priceListId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/pricing/lists/${priceListId}/archive`, {
    method: "POST",
  })
}

export async function activatePriceEntry(
  priceListId: string,
  priceEntryId: string,
): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/pricing/lists/${priceListId}/entries/${priceEntryId}/activate`,
    {
      method: "POST",
    },
  )
}

export async function deactivatePriceEntry(
  priceListId: string,
  priceEntryId: string,
): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/pricing/lists/${priceListId}/entries/${priceEntryId}/deactivate`,
    {
      method: "POST",
    },
  )
}

export async function setProductPrice(
  priceListId: string,
  productId: string,
  request: SetProductPriceRequest,
): Promise<void> {
  await putJson<void, SetProductPriceRequest>(
    `/api/stores/me/pricing/lists/${priceListId}/products/${productId}`,
    request,
  )
}

export async function removeProductPrice(
  priceListId: string,
  productId: string,
): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/pricing/lists/${priceListId}/products/${productId}`,
    {
      method: "DELETE",
    },
  )
}

export async function setVariantPrice(
  priceListId: string,
  productId: string,
  productVariantId: string,
  request: SetVariantPriceRequest,
): Promise<void> {
  await putJson<void, SetVariantPriceRequest>(
    `/api/stores/me/pricing/lists/${priceListId}/products/${productId}/variants/${productVariantId}`,
    request,
  )
}

export async function removeVariantPrice(
  priceListId: string,
  productId: string,
  productVariantId: string,
): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/pricing/lists/${priceListId}/products/${productId}/variants/${productVariantId}`,
    {
      method: "DELETE",
    },
  )
}

export async function searchStorePayments(
  options: PaymentSearchOptions,
): Promise<ApiPagedResult<PaymentSummaryDto>> {
  return fetchJson<ApiPagedResult<PaymentSummaryDto>>(
    withQuery("/api/stores/me/payments", {
      status: options.status,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getStorePaymentById(paymentId: string): Promise<PaymentDto> {
  return fetchJson<PaymentDto>(`/api/stores/me/payments/${paymentId}`)
}

export async function getIyzicoPaymentProviderAccount(): Promise<IyzicoPaymentProviderAccountDto | null> {
  try {
    return await fetchJson<IyzicoPaymentProviderAccountDto>(
      "/api/stores/me/payment-provider-accounts/iyzico",
    )
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }

    throw error
  }
}

export async function updateIyzicoPaymentProviderAccount(
  request: UpsertIyzicoPaymentProviderAccountRequest,
): Promise<IyzicoPaymentProviderAccountDto> {
  return putJson<
    IyzicoPaymentProviderAccountDto,
    UpsertIyzicoPaymentProviderAccountRequest
  >("/api/stores/me/payment-provider-accounts/iyzico", request)
}

export async function disableIyzicoPaymentProviderAccount(): Promise<IyzicoPaymentProviderAccountDto> {
  return postJson<IyzicoPaymentProviderAccountDto, Record<string, never>>(
    "/api/stores/me/payment-provider-accounts/iyzico/disable",
    {},
  )
}

export async function captureStorePayment(
  paymentId: string,
  request: CapturePaymentRequest,
): Promise<void> {
  await postJson<void, CapturePaymentRequest>(
    `/api/stores/me/payments/${paymentId}/capture`,
    request,
  )
}

export async function cancelStorePayment(
  paymentId: string,
  request: CancelPaymentRequest,
): Promise<void> {
  await postJson<void, CancelPaymentRequest>(
    `/api/stores/me/payments/${paymentId}/cancel`,
    request,
  )
}

export async function refundStorePayment(
  paymentId: string,
  request: RefundPaymentRequest,
): Promise<void> {
  await postJson<void, RefundPaymentRequest>(
    `/api/stores/me/payments/${paymentId}/refund`,
    request,
  )
}

export async function searchStoreShipments(
  options: ShipmentSearchOptions,
): Promise<ApiPagedResult<ShipmentSummaryDto>> {
  return fetchJson<ApiPagedResult<ShipmentSummaryDto>>(
    withQuery("/api/stores/me/shipments", {
      status: options.status,
      orderId: options.orderId,
      orderNumber: options.orderNumber,
      shipmentNumber: options.shipmentNumber,
      trackingNumber: options.trackingNumber,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getStoreShipmentById(shipmentId: string): Promise<ShipmentDto> {
  return fetchJson<ShipmentDto>(`/api/stores/me/shipments/${shipmentId}`)
}

export async function addShipmentPackage(
  shipmentId: string,
  request: AddShipmentPackageRequest,
): Promise<void> {
  await postJson<void, AddShipmentPackageRequest>(
    `/api/stores/me/shipments/${shipmentId}/packages`,
    request,
  )
}

export async function assignShipmentCarrier(
  shipmentId: string,
  request: AssignShipmentCarrierRequest,
): Promise<void> {
  await putJson<void, AssignShipmentCarrierRequest>(
    `/api/stores/me/shipments/${shipmentId}/carrier`,
    request,
  )
}

export async function markShipmentReady(shipmentId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/shipments/${shipmentId}/ready`, {
    method: "POST",
  })
}

export async function markShipmentShipped(shipmentId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/shipments/${shipmentId}/ship`, {
    method: "POST",
  })
}

export async function registerShipmentTrackingEvent(
  shipmentId: string,
  request: RegisterShipmentTrackingEventRequest,
): Promise<void> {
  await postJson<void, RegisterShipmentTrackingEventRequest>(
    `/api/stores/me/shipments/${shipmentId}/tracking-events`,
    request,
  )
}

export async function markShipmentDelivered(shipmentId: string): Promise<void> {
  await fetchJson<void>(`/api/stores/me/shipments/${shipmentId}/deliver`, {
    method: "POST",
  })
}

export async function cancelShipment(
  shipmentId: string,
  request: CancelShipmentRequest,
): Promise<void> {
  await postJson<void, CancelShipmentRequest>(
    `/api/stores/me/shipments/${shipmentId}/cancel`,
    request,
  )
}

export async function searchCustomers(
  options: CustomerSearchOptions,
): Promise<ApiPagedResult<CustomerSummaryDto>> {
  return fetchJson<ApiPagedResult<CustomerSummaryDto>>(
    withQuery("/api/customers", {
      searchTerm: options.searchTerm,
      status: options.status,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getCustomerById(customerId: string): Promise<CustomerDto> {
  return fetchJson<CustomerDto>(`/api/customers/${customerId}`)
}

export async function blockCustomer(customerId: string): Promise<void> {
  await fetchJson<void>(`/api/customers/${customerId}/block`, { method: "POST" })
}

export async function activateCustomer(customerId: string): Promise<void> {
  await fetchJson<void>(`/api/customers/${customerId}/activate`, {
    method: "POST",
  })
}

export async function searchNotificationTemplates(
  options: NotificationTemplateSearchOptions,
): Promise<NotificationTemplateSummaryDto[]> {
  return fetchJson<NotificationTemplateSummaryDto[]>(
    withQuery("/api/stores/me/notification-templates", {
      trigger: options.trigger,
      channel: options.channel,
      isActive: options.isActive,
    }),
  )
}

export async function getNotificationTemplateById(
  templateId: string,
): Promise<NotificationTemplateDto> {
  return fetchJson<NotificationTemplateDto>(
    `/api/stores/me/notification-templates/${templateId}`,
  )
}

export async function createNotificationTemplate(
  request: CreateNotificationTemplateRequest,
): Promise<void> {
  await postJson<void, CreateNotificationTemplateRequest>(
    "/api/stores/me/notification-templates",
    request,
  )
}

export async function updateNotificationTemplate(
  templateId: string,
  request: UpdateNotificationTemplateRequest,
): Promise<void> {
  await putJson<void, UpdateNotificationTemplateRequest>(
    `/api/stores/me/notification-templates/${templateId}`,
    request,
  )
}

export async function activateNotificationTemplate(templateId: string): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/notification-templates/${templateId}/activate`,
    {
      method: "POST",
    },
  )
}

export async function deactivateNotificationTemplate(templateId: string): Promise<void> {
  await fetchJson<void>(
    `/api/stores/me/notification-templates/${templateId}/deactivate`,
    {
      method: "POST",
    },
  )
}

export async function searchNotificationDispatches(
  options: NotificationDispatchSearchOptions,
): Promise<ApiPagedResult<NotificationDispatchSummaryDto>> {
  return fetchJson<ApiPagedResult<NotificationDispatchSummaryDto>>(
    withQuery("/api/stores/me/notifications", {
      trigger: options.trigger,
      channel: options.channel,
      status: options.status,
      businessEntityType: options.businessEntityType,
      businessEntityId: options.businessEntityId,
      pageNumber: options.pageNumber ?? 1,
      pageSize: options.pageSize ?? 20,
    }),
  )
}

export async function getNotificationDispatchById(
  dispatchId: string,
): Promise<NotificationDispatchDto> {
  return fetchJson<NotificationDispatchDto>(
    `/api/stores/me/notifications/${dispatchId}`,
  )
}
