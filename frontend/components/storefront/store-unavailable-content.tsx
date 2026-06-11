import Link from "next/link"

import { Footer } from "@/components/footer"
import { Header } from "@/components/header"

export function StoreUnavailableContent() {
  return (
    <div className="flex min-h-screen flex-col bg-background">
      <Header />

      <main className="flex flex-1 items-center justify-center pt-32">
        <div className="mx-auto max-w-2xl px-8 py-16 text-center">
          <div className="mb-8">
            <div className="mb-6 text-7xl font-light">404</div>
            <h1 className="mb-4 text-4xl font-light tracking-tight">
              Store Unavailable
            </h1>
            <p className="text-lg leading-relaxed text-muted-foreground">
              The storefront you requested does not exist, is unpublished, or is currently unavailable.
            </p>
          </div>

          <div className="flex flex-wrap justify-center gap-4">
            <Link
              href="/"
              className="border border-black px-8 py-4 text-sm font-light uppercase tracking-widest transition-colors hover:bg-black hover:text-white"
            >
              Return Home
            </Link>
            <Link
              href="/contact"
              className="bg-black px-8 py-4 text-sm font-light uppercase tracking-widest text-white transition-colors hover:bg-gray-800"
            >
              Contact Support
            </Link>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  )
}
