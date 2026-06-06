"use client"

import { useState } from "react"
import Link from "next/link"
import { Eye, EyeOff } from "lucide-react"

import { ApiError } from "@/lib/api/client"
import { loginCustomer } from "@/lib/api/auth"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"
import { getStoreDisplayName } from "@/lib/store-branding"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"

interface LoginPageContentProps {
  storeSlug?: string
  storeName?: string | null
  loginPageImageUrl?: string | null
  nextPath?: string
  allowInactiveStore?: boolean
}

const DEFAULT_LOGIN_PAGE_IMAGE_URL =
  "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?w=1200&h=1600&fit=crop"

export function LoginPageContent({
  storeSlug,
  storeName,
  loginPageImageUrl,
  nextPath = "/account",
  allowInactiveStore = false,
}: LoginPageContentProps) {
  const resolvedStoreSlug = storeSlug ?? defaultStoreSlug
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [isPersistent, setIsPersistent] = useState(false)
  const [formData, setFormData] = useState({
    email: "",
    password: "",
  })

  const homeHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug) : "/"
  const displayName = getStoreDisplayName(storeName, resolvedStoreSlug)
  const forgotPasswordHref = resolvedStoreSlug
    ? storefrontPath(resolvedStoreSlug, "/forgot-password")
    : "/auth/forgot-password"
  const registerHref = resolvedStoreSlug
    ? storefrontPath(resolvedStoreSlug, "/register")
    : "/auth/register"
  const resolvedLoginPageImageUrl =
    loginPageImageUrl?.trim() || DEFAULT_LOGIN_PAGE_IMAGE_URL

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrorMessage(null)

    if (!resolvedStoreSlug) {
      setErrorMessage("Store context could not be resolved for login.")
      return
    }

    setIsLoading(true)

    try {
      await loginCustomer({
        storeSlug: resolvedStoreSlug,
        email: formData.email.trim(),
        password: formData.password,
        isPersistent,
        allowInactiveStore,
      })

      window.location.assign(nextPath)
    } catch (error) {
      if (error instanceof ApiError) {
        const detail =
          typeof error.payload === "object" &&
          error.payload !== null &&
          "detail" in error.payload
            ? String(error.payload.detail)
            : error.message

        setErrorMessage(detail)
      } else {
        setErrorMessage("Login could not be completed.")
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className="min-h-screen bg-background flex">
      <div className="flex-1 flex items-center justify-center px-6 py-12">
        <div className="w-full max-w-md">
          <div className="mb-12">
            <Link
              href={homeHref}
              className="font-serif text-2xl tracking-[0.3em] font-light"
            >
              {displayName}
            </Link>
          </div>

          <div className="mb-10">
            <h1 className="font-serif text-3xl lg:text-4xl font-light tracking-wide mb-4">
              Welcome Back
            </h1>
            <p className="text-muted-foreground">
              Sign in to your account to continue.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                Email Address
              </label>
              <Input
                type="email"
                required
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
                className="h-14 bg-secondary border-0 text-sm tracking-wide"
                placeholder="your@email.com"
              />
            </div>

            <div>
              <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                Password
              </label>
              <div className="relative">
                <Input
                  type={showPassword ? "text" : "password"}
                  required
                  value={formData.password}
                  onChange={(e) =>
                    setFormData({ ...formData, password: e.target.value })
                  }
                  className="h-14 bg-secondary border-0 text-sm tracking-wide pr-12"
                  placeholder="Enter your password"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                >
                  {showPassword ? (
                    <EyeOff className="h-5 w-5" strokeWidth={1} />
                  ) : (
                    <Eye className="h-5 w-5" strokeWidth={1} />
                  )}
                </button>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={isPersistent}
                  onChange={(e) => setIsPersistent(e.target.checked)}
                  className="w-4 h-4 border-border accent-primary"
                />
                <span className="text-sm text-muted-foreground">
                  Remember me
                </span>
              </label>
              <Link
                href={forgotPasswordHref}
                className="text-sm text-muted-foreground hover:text-foreground transition-colors"
              >
                Forgot password?
              </Link>
            </div>

            {errorMessage ? (
              <p className="text-sm text-red-600">{errorMessage}</p>
            ) : null}

            <Button
              type="submit"
              disabled={isLoading}
              className="w-full h-14 bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase hover:bg-primary/90 transition-colors"
            >
              {isLoading ? "Signing in..." : "Sign In"}
            </Button>
          </form>

          <div className="mt-8 text-center">
            <p className="text-sm text-muted-foreground">
              Don&apos;t have an account?{" "}
              <Link
                href={registerHref}
                className="text-foreground underline underline-offset-4"
              >
                Create one
              </Link>
            </p>
          </div>

          <div className="my-10 flex items-center gap-4">
            <div className="flex-1 h-px bg-border" />
            <span className="text-xs text-muted-foreground tracking-wide">
              OR
            </span>
            <div className="flex-1 h-px bg-border" />
          </div>

          <div className="space-y-4">
            <button className="w-full h-14 border border-border text-sm tracking-wide hover:bg-secondary transition-colors flex items-center justify-center gap-3">
              <svg className="h-5 w-5" viewBox="0 0 24 24">
                <path
                  fill="currentColor"
                  d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                />
                <path
                  fill="currentColor"
                  d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                />
                <path
                  fill="currentColor"
                  d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                />
                <path
                  fill="currentColor"
                  d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                />
              </svg>
              Continue with Google
            </button>
          </div>
        </div>
      </div>

      <div className="hidden lg:block flex-1 relative">
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage: `url('${resolvedLoginPageImageUrl}')`,
          }}
        >
          <div className="absolute inset-0 bg-black/20" />
        </div>
      </div>
    </main>
  )
}
