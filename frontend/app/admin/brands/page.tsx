import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminBrandCreateForm } from "@/components/admin/admin-create-forms"
import { searchBrands } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime } from "@/lib/admin-format"

type Props = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>
}

function getValue(
  searchParams: Record<string, string | string[] | undefined>,
  key: string,
): string {
  const value = searchParams[key]
  return typeof value === "string" ? value : ""
}

export default async function BrandsPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q")

  try {
    const brands = await searchBrands(query || undefined)

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Brands</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Brand cards now use the real backend shape instead of mock website and featured fields.
          </p>
        </div>

        <AdminBrandCreateForm />

        <form className="max-w-md">
          <input
            type="text"
            name="q"
            defaultValue={query}
            placeholder="Search brands by name"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </form>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Brand</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Slug</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Updated</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Description</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {brands.map((brand) => (
                  <tr key={brand.id} className="hover:bg-secondary/30">
                    <td className="p-4 text-sm font-medium">{brand.name}</td>
                    <td className="p-4 text-sm text-muted-foreground">/{brand.slug}</td>
                    <td className="p-4 text-sm">{brand.isActive ? "Active" : "Inactive"}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(brand.updatedAtUtc)}
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {brand.description ?? "No description"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    )
  } catch (error) {
    return (
      <AdminErrorState
        title="Brands could not be loaded"
        message={getApiErrorMessage(error, "The brand search request failed.")}
      />
    )
  }
}
