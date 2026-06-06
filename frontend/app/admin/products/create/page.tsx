import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminProductEditor } from "@/components/admin/admin-product-editor"
import { getCategoryTree, listAttributeDefinitions, searchBrands } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

export default async function CreateProductPage() {
  try {
    const [brands, categories, attributes] = await Promise.all([
      searchBrands(),
      getCategoryTree(),
      listAttributeDefinitions(true),
    ])

    return (
      <AdminProductEditor
        brands={brands}
        categories={categories}
        attributes={attributes}
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
