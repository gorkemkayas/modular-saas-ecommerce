import { notFound } from "next/navigation"

import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminProductEditor } from "@/components/admin/admin-product-editor"
import {
  getCategoryTree,
  getProductById,
  listAttributeDefinitions,
  searchBrands,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

type Props = {
  searchParams: Promise<{ id?: string }>
}

export default async function EditProductPage({ searchParams }: Props) {
  const { id } = await searchParams

  if (!id) {
    notFound()
  }

  try {
    const [brands, categories, product, attributes] = await Promise.all([
      searchBrands(),
      getCategoryTree(),
      getProductById(id),
      listAttributeDefinitions(true),
    ])

    return (
      <AdminProductEditor
        brands={brands}
        categories={categories}
        attributes={attributes}
        initialProduct={product}
      />
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Product edit form could not be loaded"
        message={getApiErrorMessage(error, "The product editor request failed.")}
      />
    )
  }
}
