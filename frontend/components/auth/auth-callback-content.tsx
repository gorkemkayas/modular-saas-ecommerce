"use client"

import { useEffect } from "react"
import Link from "next/link"
import { useRouter, useSearchParams } from "next/navigation"
import { Loader2 } from "lucide-react"
import { Footer } from "@/components/footer"
import { Header } from "@/components/header"

export function AuthCallbackContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const token = searchParams.get("token")
  const code = searchParams.get("code")
  const error = searchParams.get("error")

  useEffect(() => {
    if (error) {
      const timer = window.setTimeout(() => {
        router.push(`/auth/login?error=${encodeURIComponent(error)}`)
      }, 2000)

      return () => window.clearTimeout(timer)
    }

    if (token || code) {
      if (token) {
        window.localStorage.setItem("authToken", token)
      }

      const timer = window.setTimeout(() => {
        router.push("/account")
      }, 2000)

      return () => window.clearTimeout(timer)
    }
  }, [token, code, error, router])

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <Header />

      <main className="flex flex-1 items-center justify-center pt-32">
        <div className="mx-auto max-w-2xl px-8 py-16 text-center">
          <Loader2 className="mx-auto mb-8 h-12 w-12 animate-spin text-black" strokeWidth={1} />

          {error ? (
            <>
              <h1 className="mb-4 text-3xl font-light tracking-tight">Authentication Failed</h1>
              <p className="mb-8 text-gray-600">
                {error === "access_denied"
                  ? "You denied the authorization request."
                  : "An error occurred during authentication."}
              </p>
              <Link
                href="/auth/login"
                className="inline-block border border-black px-8 py-4 text-sm font-light uppercase tracking-widest transition-colors hover:bg-black hover:text-white"
              >
                Try Again
              </Link>
            </>
          ) : (
            <>
              <h1 className="mb-4 text-3xl font-light tracking-tight">Completing Sign In</h1>
              <p className="text-gray-600">Please wait while we process your authentication...</p>
            </>
          )}
        </div>
      </main>

      <Footer />
    </div>
  )
}
