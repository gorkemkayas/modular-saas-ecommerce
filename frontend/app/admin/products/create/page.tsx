import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminProductEditor } from "@/components/admin/admin-product-editor"
import {
  getCategoryTree,
  listAttributeDefinitions,
  searchBrands,
  searchProducts,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { getCurrentSubscriptionOrNull } from "@/lib/api/subscription"

export default async function CreateProductPage() {
  try {
    const [
      brands,
      categories,
      attributes,
      subscription,
      draftProducts,
      activeProducts,
    ] = await Promise.all([
      searchBrands(),
      getCategoryTree(),
      listAttributeDefinitions(true),
      getCurrentSubscriptionOrNull(),
      searchProducts({ status: "Draft", pageNumber: 1, pageSize: 1 }),
      searchProducts({ status: "Active", pageNumber: 1, pageSize: 1 }),
    ])
    const currentProductCount = draftProducts.totalCount + activeProducts.totalCount

    return (
      <AdminProductEditor
        brands={brands}
        categories={categories}
        attributes={attributes}
        subscription={subscription}
        currentProductCount={currentProductCount}
      />
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Product create form could not be loaded"
        message={getApiErrorMessage(error, "The product editor dependencies failed to load.")}
      />
    )
  }
}
