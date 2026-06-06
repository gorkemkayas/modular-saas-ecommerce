import Link from "next/link"

import { withQuery } from "@/lib/config"

export function AdminPagination({
  basePath,
  currentPage,
  totalPages,
  query,
}: {
  basePath: string
  currentPage: number
  totalPages: number
  query: Record<string, string | number | boolean | null | undefined>
}) {
  if (totalPages <= 1) {
    return null
  }

  const previousPage = currentPage - 1
  const nextPage = currentPage + 1

  return (
    <div className="flex items-center justify-between gap-4">
      <p className="text-sm text-muted-foreground">
        Page {currentPage} / {totalPages}
      </p>
      <div className="flex items-center gap-2">
        <Link
          href={withQuery(basePath, { ...query, page: previousPage })}
          className={`border px-3 py-2 text-sm transition-colors ${
            currentPage <= 1
              ? "pointer-events-none border-border/50 text-muted-foreground/50"
              : "border-border hover:bg-secondary"
          }`}
        >
          Previous
        </Link>
        <Link
          href={withQuery(basePath, { ...query, page: nextPage })}
          className={`border px-3 py-2 text-sm transition-colors ${
            currentPage >= totalPages
              ? "pointer-events-none border-border/50 text-muted-foreground/50"
              : "border-border hover:bg-secondary"
          }`}
        >
          Next
        </Link>
      </div>
    </div>
  )
}
