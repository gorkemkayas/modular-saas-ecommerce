import { AdminErrorState } from "@/components/admin/admin-error-state"
import { AdminAttributeCreateForm } from "@/components/admin/admin-create-forms"
import { listAttributeDefinitions } from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { formatDateTime, formatEnumLabel } from "@/lib/admin-format"

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

export default async function AttributesPage({ searchParams }: Props) {
  const resolvedSearchParams = searchParams ? await searchParams : {}
  const query = getValue(resolvedSearchParams, "q").trim().toLowerCase()

  try {
    const attributes = (await listAttributeDefinitions(false)).filter((attribute) =>
      query
        ? attribute.name.toLowerCase().includes(query) ||
          attribute.code.toLowerCase().includes(query)
        : true,
    )

    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-light tracking-wide">Attributes</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Attribute management now reflects the backend definition model exactly.
          </p>
        </div>

        <AdminAttributeCreateForm />

        <form className="max-w-md">
          <input
            type="text"
            name="q"
            defaultValue={query}
            placeholder="Search by name or code"
            className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
          />
        </form>

        <div className="border border-border overflow-x-auto">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border bg-secondary/50">
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Name</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Code</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Type</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Flags</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Status</th>
                  <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Updated</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {attributes.map((attribute) => (
                  <tr key={attribute.id} className="hover:bg-secondary/30">
                    <td className="p-4 text-sm font-medium">{attribute.name}</td>
                    <td className="p-4 text-sm text-muted-foreground">{attribute.code}</td>
                    <td className="p-4 text-sm">{formatEnumLabel(attribute.dataType)}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {[
                        attribute.isRequired ? "Required" : null,
                        attribute.isFilterable ? "Filterable" : null,
                        attribute.isVariantDefining ? "Variant defining" : null,
                      ]
                        .filter(Boolean)
                        .join(" • ") || "No special flags"}
                    </td>
                    <td className="p-4 text-sm">
                      {attribute.isActive ? "Active" : "Inactive"}
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {formatDateTime(attribute.updatedAtUtc)}
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
        title="Attributes could not be loaded"
        message={getApiErrorMessage(error, "The attribute list request failed.")}
      />
    )
  }
}
