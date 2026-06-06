import { fetchJson } from "@/lib/api/client"
import type {
  ApiPagedResult,
  StorefrontBrandDto,
  StorefrontCatalogFacetsDto,
  StorefrontCategoryTreeNodeDto,
  StorefrontDto,
  StorefrontProductDto,
  StorefrontProductSummaryDto,
} from "@/lib/api/types"
import { withQuery } from "@/lib/config"

export interface StorefrontSearchParams {
  searchTerm?: string
  categoryId?: string
  brandId?: string
  currencyCode?: string
  pageNumber?: number
  pageSize?: number
}

export async function getStorefront(storeSlug: string): Promise<StorefrontDto> {
  return fetchJson<StorefrontDto>(`/api/storefront/${storeSlug}`)
}

export async function getStorefrontProducts(
  storeSlug: string,
  params: StorefrontSearchParams = {},
): Promise<ApiPagedResult<StorefrontProductSummaryDto>> {
  return fetchJson<ApiPagedResult<StorefrontProductSummaryDto>>(
    withQuery(`/api/storefront/${storeSlug}/products`, {
      searchTerm: params.searchTerm,
      categoryId: params.categoryId,
      brandId: params.brandId,
      currencyCode: params.currencyCode ?? "TRY",
      pageNumber: params.pageNumber ?? 1,
      pageSize: params.pageSize ?? 20,
    }),
  )
}

export async function getStorefrontProductBySlug(
  storeSlug: string,
  productSlug: string,
  currencyCode = "TRY",
): Promise<StorefrontProductDto> {
  return fetchJson<StorefrontProductDto>(
    withQuery(`/api/storefront/${storeSlug}/products/${productSlug}`, {
      currencyCode,
    }),
  )
}

export async function getStorefrontCategoryTree(
  storeSlug: string,
): Promise<StorefrontCategoryTreeNodeDto[]> {
  return fetchJson<StorefrontCategoryTreeNodeDto[]>(
    `/api/storefront/${storeSlug}/categories/tree`,
  )
}

export async function getStorefrontBrands(
  storeSlug: string,
  searchTerm?: string,
): Promise<StorefrontBrandDto[]> {
  return fetchJson<StorefrontBrandDto[]>(
    withQuery(`/api/storefront/${storeSlug}/brands`, { searchTerm }),
  )
}

export async function getStorefrontFacets(
  storeSlug: string,
  params: Pick<StorefrontSearchParams, "searchTerm" | "categoryId" | "brandId"> = {},
): Promise<StorefrontCatalogFacetsDto> {
  return fetchJson<StorefrontCatalogFacetsDto>(
    withQuery(`/api/storefront/${storeSlug}/facets`, params),
  )
}
