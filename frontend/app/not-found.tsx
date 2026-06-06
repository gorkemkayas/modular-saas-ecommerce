import Link from "next/link"
import { Footer } from "@/components/footer"
import { Header } from "@/components/header"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"

export default function NotFoundPage() {
  const homeHref = defaultStoreSlug ? storefrontPath(defaultStoreSlug) : "/"
  const productsHref = defaultStoreSlug
    ? storefrontPath(defaultStoreSlug, "/products")
    : "/"

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <Header storeSlug={defaultStoreSlug ?? undefined} />

      <main className="flex flex-1 items-center justify-center pt-32">
        <div className="mx-auto max-w-2xl px-8 py-16 text-center">
          <div className="mb-8">
            <div className="mb-6 text-7xl font-light">404</div>
            <h1 className="mb-4 text-4xl font-light tracking-tight">Page Not Found</h1>
            <p className="text-lg leading-relaxed text-muted-foreground">
              The page you are looking for does not exist or may have moved.
            </p>
          </div>

          <div className="flex flex-wrap justify-center gap-4">
            <Link
              href={homeHref}
              className="px-8 py-4 border border-black hover:bg-black hover:text-white transition-colors font-light tracking-widest uppercase text-sm"
            >
              Return Home
            </Link>
            <Link
              href={productsHref}
              className="px-8 py-4 bg-black text-white hover:bg-gray-800 transition-colors font-light tracking-widest uppercase text-sm"
            >
              Browse Products
            </Link>
          </div>
        </div>
      </main>

      <Footer storeSlug={defaultStoreSlug ?? undefined} />
    </div>
  )
}
