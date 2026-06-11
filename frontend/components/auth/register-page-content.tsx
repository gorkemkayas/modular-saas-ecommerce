"use client"

import { useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Check, CircleAlert, Eye, EyeOff } from "lucide-react"

import { ApiError } from "@/lib/api/client"
import { registerCustomer } from "@/lib/api/auth"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"
import { getStoreDisplayName } from "@/lib/store-branding"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"

interface RegisterPageContentProps {
  storeSlug?: string
  storeName?: string | null
  registerPageImageUrl?: string | null
  nextPath?: string
}

const DEFAULT_REGISTER_PAGE_IMAGE_URL =
  "/images/platform/store-setup-register.png"

export function RegisterPageContent({
  storeSlug,
  storeName,
  registerPageImageUrl,
}: RegisterPageContentProps) {
  const router = useRouter()
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    acceptTerms: false,
    newsletter: false,
  })

  const resolvedStoreSlug = storeSlug ?? defaultStoreSlug ?? undefined
  const displayName = getStoreDisplayName(storeName, resolvedStoreSlug)
  const homeHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug) : "/"
  const loginHref = resolvedStoreSlug
    ? storefrontPath(resolvedStoreSlug, "/login")
    : "/auth/login"
  const resolvedRegisterPageImageUrl =
    registerPageImageUrl?.trim() || DEFAULT_REGISTER_PAGE_IMAGE_URL

  const passwordRequirements = [
    { label: "At least 8 characters", met: formData.password.length >= 8 },
    { label: "Contains uppercase letter", met: /[A-Z]/.test(formData.password) },
    { label: "Contains number", met: /[0-9]/.test(formData.password) },
  ]

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!resolvedStoreSlug) {
      setErrorMessage("Store context could not be resolved for registration.")
      return
    }

    setErrorMessage(null)
    setIsLoading(true)

    try {
      await registerCustomer({
        storeSlug: resolvedStoreSlug,
        email: formData.email.trim(),
        password: formData.password,
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
      })

      router.push(loginHref)
    } catch (error) {
      setErrorMessage(getRegisterErrorMessage(error))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className="min-h-screen bg-background flex">
      <div className="hidden lg:block flex-1 relative">
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage: `url('${resolvedRegisterPageImageUrl}')`,
          }}
        >
          <div className="absolute inset-0 bg-black/20" />
        </div>
      </div>

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
              Create Account
            </h1>
            <p className="text-muted-foreground">
              Join us and discover our premium collection.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            {errorMessage ? (
              <Alert className="border-red-200/70 bg-red-50/70 text-red-950 shadow-sm [&>svg]:text-red-700">
                <CircleAlert />
                <AlertTitle className="text-sm font-medium tracking-[0.08em] uppercase">
                  Account Not Created
                </AlertTitle>
                <AlertDescription className="text-sm leading-relaxed text-red-900/85">
                  <p>{errorMessage}</p>
                </AlertDescription>
              </Alert>
            ) : null}

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                  First Name
                </label>
                <Input
                  type="text"
                  required
                  value={formData.firstName}
                  onChange={(e) =>
                    setFormData({ ...formData, firstName: e.target.value })
                  }
                  className="h-14 bg-secondary border-0 text-sm tracking-wide"
                  placeholder="John"
                />
              </div>
              <div>
                <label className="block text-xs tracking-[0.2em] uppercase mb-3">
                  Last Name
                </label>
                <Input
                  type="text"
                  required
                  value={formData.lastName}
                  onChange={(e) =>
                    setFormData({ ...formData, lastName: e.target.value })
                  }
                  className="h-14 bg-secondary border-0 text-sm tracking-wide"
                  placeholder="Doe"
                />
              </div>
            </div>

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
                  placeholder="Create a password"
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
              {formData.password ? (
                <div className="mt-3 space-y-2">
                  {passwordRequirements.map((req) => (
                    <div
                      key={req.label}
                      className={`flex items-center gap-2 text-xs ${
                        req.met ? "text-foreground" : "text-muted-foreground"
                      }`}
                    >
                      <Check
                        className={`h-3 w-3 ${
                          req.met ? "opacity-100" : "opacity-30"
                        }`}
                        strokeWidth={1.5}
                      />
                      {req.label}
                    </div>
                  ))}
                </div>
              ) : null}
            </div>

            <div className="space-y-4">
              <label className="flex items-start gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  required
                  checked={formData.acceptTerms}
                  onChange={(e) =>
                    setFormData({ ...formData, acceptTerms: e.target.checked })
                  }
                  className="w-4 h-4 mt-0.5 border-border accent-primary"
                />
                <span className="text-sm text-muted-foreground leading-relaxed">
                  I agree to the{" "}
                  <Link
                    href="/terms"
                    className="text-foreground underline underline-offset-4"
                  >
                    Terms of Service
                  </Link>{" "}
                  and{" "}
                  <Link
                    href="/privacy-policy"
                    className="text-foreground underline underline-offset-4"
                  >
                    Privacy Policy
                  </Link>
                </span>
              </label>

              <label className="flex items-start gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={formData.newsletter}
                  onChange={(e) =>
                    setFormData({ ...formData, newsletter: e.target.checked })
                  }
                  className="w-4 h-4 mt-0.5 border-border accent-primary"
                />
                <span className="text-sm text-muted-foreground leading-relaxed">
                  Subscribe to our newsletter for exclusive offers and updates
                </span>
              </label>
            </div>

            <Button
              type="submit"
              disabled={isLoading}
              className="w-full h-14 bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase hover:bg-primary/90 transition-colors"
            >
              {isLoading ? "Creating account..." : "Create Account"}
            </Button>
          </form>

          <div className="mt-8 text-center">
            <p className="text-sm text-muted-foreground">
              Already have an account?{" "}
              <Link
                href={loginHref}
                className="text-foreground underline underline-offset-4"
              >
                Sign in
              </Link>
            </p>
          </div>
        </div>
      </div>
    </main>
  )
}

function getRegisterErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return "We couldn't create your account right now. Please try again."
  }

  if (typeof error.payload === "string" && error.payload.trim()) {
    const parsedPayload = tryParseProblemPayload(error.payload)

    if (parsedPayload) {
      return readProblemMessage(parsedPayload) ?? error.payload
    }

    return error.payload
  }

  if (error.payload && typeof error.payload === "object") {
    const message = readProblemMessage(error.payload as Record<string, unknown>)

    if (message) {
      return message
    }
  }

  if (error.status === 409) {
    return "An account with this email address already exists."
  }

  if (error.status === 404) {
    return "This store is not available right now."
  }

  return "We couldn't create your account right now. Please try again."
}

function readValidationMessage(errors: unknown): string | null {
  if (!errors || typeof errors !== "object") {
    return null
  }

  const messages = Object.values(errors as Record<string, unknown>)
    .flatMap((value) => (Array.isArray(value) ? value : []))
    .filter((value): value is string => typeof value === "string" && value.trim().length > 0)

  return messages.length > 0 ? messages.join(" ") : null
}

function readProblemMessage(payload: Record<string, unknown>): string | null {
  const validationMessage = readValidationMessage(payload.errors)

  if (validationMessage) {
    return validationMessage
  }

  if (typeof payload.detail === "string" && payload.detail.trim()) {
    return payload.detail
  }

  if (typeof payload.title === "string" && payload.title.trim()) {
    return payload.title
  }

  return null
}

function tryParseProblemPayload(value: string): Record<string, unknown> | null {
  try {
    const parsed = JSON.parse(value)
    return parsed && typeof parsed === "object"
      ? (parsed as Record<string, unknown>)
      : null
  } catch {
    return null
  }
}
