import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminCategoryManager } from "@/components/admin/admin-category-manager"
import { AdminCategoryCreateForm } from "@/components/admin/admin-create-forms"
import { getCategoryTree } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { getCurrentSubscriptionOrNull } from "@/lib/api/subscription"

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>
}

type CategoryRow = {
  id: string
  name: string
  slug: string
  depth: number
  isActive: boolean
  childCount: number
  description: string | null
  imageUrl: string | null
  parentCategoryId: string | null
  sortOrder: number
}

function getValue(
  searchParams: Record<string, string | string[] | undefined>,
  key: string,
): string {
  const value = searchParams[key]
  return typeof value === "string" ? value : ""
}

function flattenCategories(
  nodes: Awaited<ReturnType<typeof getCategoryTree>>,
  depth = 0,
): CategoryRow[] {
  return nodes.flatMap((node) => [
    {
      id: node.id,
      name: node.name,
      slug: node.slug,
      depth,
      isActive: node.isActive,
      childCount: node.children.length,
      description: node.description,
      imageUrl: node.imageUrl,
      parentCategoryId: node.parentCategoryId,
      sortOrder: node.sortOrder,
    },
    ...flattenCategories(node.children, depth + 1),
  ])
}

export default async function CategoriesPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q").trim().toLowerCase()

  try {
    const [tree, subscription] = await Promise.all([
      getCategoryTree(),
      getCurrentSubscriptionOrNull(),
    ])
    const allRows = flattenCategories(tree)
    const activeCategoryCount = allRows.filter((category) => category.isActive).length
    const rows = allRows.filter((category) =>
      query
        ? category.name.toLowerCase().includes(query) ||
          category.slug.toLowerCase().includes(query)
        : true,
    )

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Categories</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            This screen follows the backend tree model from `api/stores/me/categories/tree`.
          </p>
        </div>

        <AdminCategoryCreateForm
          categories={allRows.map((category) => ({
            id: category.id,
            name: category.name,
            depth: category.depth,
          }))}
          subscription={subscription}
          currentCategoryCount={activeCategoryCount}
        />

        <form className="max-w-md">
          <input
            type="text"
            name="q"
            defaultValue={query}
            placeholder="Search by category name or slug"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </form>

        <AdminCategoryManager
          rows={rows}
          categoryOptions={allRows.map((category) => ({
            id: category.id,
            name: category.name,
            depth: category.depth,
          }))}
        />
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Categories could not be loaded"
        message={getApiErrorMessage(error, "The category tree request failed.")}
      />
    )
  }
}
