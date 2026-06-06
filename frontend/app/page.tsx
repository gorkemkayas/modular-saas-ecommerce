import Link from "next/link"
import { redirect } from "next/navigation"
import { defaultStoreSlug } from "@/lib/config"

export default function RootPage() {
  if (defaultStoreSlug) {
    redirect(`/${defaultStoreSlug}`)
  }

  return (
    <main className="min-h-screen bg-background px-6 py-24">
      <div className="mx-auto max-w-3xl">
        <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">
          Storefront Bootstrap
        </p>
        <h1 className="mt-4 font-serif text-4xl font-light tracking-tight text-foreground sm:text-5xl">
          Set a default storefront slug
        </h1>
        <p className="mt-6 text-base leading-relaxed text-muted-foreground">
          This frontend now follows the backend&apos;s multi-tenant storefront routing.
          Define <code className="rounded bg-secondary px-2 py-1">NEXT_PUBLIC_DEFAULT_STORE_SLUG</code>
          or browse directly to a published store such as <code className="rounded bg-secondary px-2 py-1">/your-store-slug</code>.
        </p>
        <div className="mt-10">
          <Link href="/admin" className="text-sm underline underline-offset-4">
            Open Admin UI
          </Link>
        </div>
      </div>
    </main>
  )
}
