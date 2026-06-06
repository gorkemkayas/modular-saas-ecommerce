"use client"

import Link from "next/link"
import { useParams } from "next/navigation"
import { Button } from "@/components/ui/button"
import { storefrontPath } from "@/lib/config"

export default function AccountRouteError({
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  const params = useParams<{ storeSlug?: string }>()
  const storeSlug = typeof params?.storeSlug === "string" ? params.storeSlug : null
  const loginHref = storeSlug ? storefrontPath(storeSlug, "/login") : "/auth/login"

  return (
    <main className="min-h-screen bg-background">
      <div className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center px-6 py-16 text-center">
        <p className="mb-4 text-xs uppercase tracking-[0.3em] text-muted-foreground">
          Account Unavailable
        </p>
        <h1 className="font-serif text-4xl font-light tracking-wide">
          We couldn&apos;t load this account page
        </h1>
        <p className="mt-4 max-w-xl text-sm text-muted-foreground">
          Your authentication may be valid, but the customer profile could not be loaded yet. Try refreshing first. If the problem continues, sign in again.
        </p>

        <div className="mt-8 flex flex-col gap-3 sm:flex-row">
          <Button
            type="button"
            onClick={reset}
            className="h-12 px-8 text-sm uppercase tracking-[0.2em]"
          >
            Retry
          </Button>
          <Button asChild variant="outline" className="h-12 px-8 text-sm tracking-wide">
            <Link href={loginHref}>Sign In Again</Link>
          </Button>
        </div>
      </div>
    </main>
  )
}
