"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import {
  FileText,
  Lock,
  LogOut,
  MapPin,
  Package,
  Settings,
  Truck,
  User,
} from "lucide-react"
import { resolveAccountBasePath } from "@/lib/account-path"
import { logoutCustomer } from "@/lib/api/auth"
import { storefrontPath } from "@/lib/config"
import { useState } from "react"

const accountLinks = [
  { path: "", label: "My Account", icon: User },
  { path: "/orders", label: "Orders", icon: Package },
  { path: "/addresses", label: "Addresses", icon: MapPin },
  { path: "/shipments", label: "Shipment Tracking", icon: Truck },
  { path: "/preferences", label: "Preferences", icon: Settings },
  { path: "/consents", label: "Consents", icon: FileText },
  { path: "/security", label: "Security", icon: Lock },
]

export function AccountLayoutShell({
  children,
}: {
  children: React.ReactNode
}) {
  const pathname = usePathname()
  const router = useRouter()
  const basePath = resolveAccountBasePath(pathname)
  const [isSigningOut, setIsSigningOut] = useState(false)

  const storeScopedMatch = /^\/([^/]+)\/account(?:\/|$)/.exec(pathname)
  const storeSlug = storeScopedMatch?.[1] ?? null
  const loginHref = storeSlug ? storefrontPath(storeSlug, "/login") : "/auth/login"

  async function handleSignOut() {
    if (isSigningOut) {
      return
    }

    setIsSigningOut(true)

    try {
      await logoutCustomer()
      router.push(loginHref)
      router.refresh()
    } finally {
      setIsSigningOut(false)
    }
  }

  return (
    <main className="min-h-screen bg-background">
      <section className="border-b border-border">
        <div className="mx-auto max-w-7xl px-6 py-16 lg:py-20">
          <p className="text-xs tracking-[0.3em] text-muted-foreground uppercase mb-4">
            Customer Account
          </p>
          <h1 className="font-serif text-4xl lg:text-5xl font-light tracking-wide">
            Manage Profile, Orders, and Delivery
          </h1>
        </div>
      </section>

      <div className="mx-auto max-w-7xl px-6 py-12">
        <div className="flex flex-col lg:flex-row gap-12">
          <aside className="lg:w-64 flex-shrink-0">
            <nav className="sticky top-24 space-y-1">
              {accountLinks.map((link) => {
                const Icon = link.icon
                const href = `${basePath}${link.path}`
                const isActive =
                  pathname === href ||
                  (link.path !== "" && pathname.startsWith(href))

                return (
                  <Link
                    key={href}
                    href={href}
                    className={`flex items-center gap-4 px-4 py-3 text-sm tracking-wide transition-colors ${
                      isActive
                        ? "bg-secondary text-foreground"
                        : "text-muted-foreground hover:text-foreground hover:bg-secondary/50"
                    }`}
                  >
                    <Icon className="h-4 w-4" strokeWidth={1} />
                    {link.label}
                  </Link>
                )
              })}
              <div className="pt-4 mt-4 border-t border-border">
                <button
                  type="button"
                  onClick={() => void handleSignOut()}
                  disabled={isSigningOut}
                  className="flex items-center gap-4 px-4 py-3 text-sm tracking-wide text-muted-foreground hover:text-foreground transition-colors w-full disabled:opacity-60"
                >
                  <LogOut className="h-4 w-4" strokeWidth={1} />
                  {isSigningOut ? "Signing Out..." : "Sign Out"}
                </button>
              </div>
            </nav>
          </aside>

          <div className="flex-1 min-w-0">{children}</div>
        </div>
      </div>
    </main>
  )
}
