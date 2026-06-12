import Link from "next/link"
import { ArrowLeft, Mail } from "lucide-react"

import { storefrontPath } from "@/lib/config"

interface ForgotPasswordPageContentProps {
  storeSlug?: string
}

export function ForgotPasswordPageContent({
  storeSlug,
}: ForgotPasswordPageContentProps) {
  const loginHref = storeSlug ? storefrontPath(storeSlug, "/login") : "/auth/login"
  const storeHref = storeSlug ? storefrontPath(storeSlug) : "/"

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4 py-10 sm:px-6 sm:py-12">
      <div className="w-full max-w-lg border border-border p-6 sm:p-8">
        <div className="mb-8 flex h-12 w-12 items-center justify-center bg-secondary">
          <Mail className="h-6 w-6" strokeWidth={1} />
        </div>

        <h1 className="font-serif text-3xl font-light tracking-wide">
          Password Reset
        </h1>
        <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
          Password reset is handled by the external authentication provider in this project.
          This placeholder route keeps the frontend navigation valid until the real auth flow is wired in.
        </p>

        <div className="mt-8 space-y-3 text-sm text-muted-foreground">
          <p>When auth integration is connected, this page should redirect to the provider reset flow.</p>
          <p>For now, return to sign in or continue back to the storefront.</p>
        </div>

        <div className="mt-8 flex flex-col gap-4 sm:flex-row sm:flex-wrap">
          <Link
            href={loginHref}
            className="inline-flex h-12 w-full items-center justify-center border border-border px-6 text-center text-sm tracking-wide transition-colors hover:bg-secondary/30 sm:w-auto"
          >
            Back to Login
          </Link>
          <Link
            href={storeHref}
            className="inline-flex h-12 w-full items-center justify-center gap-2 border border-border px-6 text-center text-sm tracking-wide transition-colors hover:bg-secondary/30 sm:w-auto"
          >
            <ArrowLeft className="h-4 w-4" strokeWidth={1} />
            Back to Store
          </Link>
        </div>
      </div>
    </main>
  )
}
