"use client"

import { useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { LayoutDashboard, Menu, Search, ShoppingBag, User, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import type { AuthSessionResponse } from "@/lib/api/auth"
import { getAuthSession, isSessionForStore } from "@/lib/api/auth"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"
import { getStoreDisplayName } from "@/lib/store-branding"
import { useStore } from "@/lib/store-context"
import { cn } from "@/lib/utils"

type HeaderSessionSnapshot = Pick<
  AuthSessionResponse,
  "isAuthenticated" | "canAccessAdmin" | "storeSlug"
>

interface HeaderProps {
  storeSlug?: string
  storeName?: string | null
  initialSession?: HeaderSessionSnapshot
}

export function Header({ storeSlug, storeName, initialSession }: HeaderProps) {
  const { getCartCount } = useStore()
  const pathname = usePathname()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const [scrolled, setScrolled] = useState(false)
  const [isAuthenticated, setIsAuthenticated] = useState(() =>
    initialSession
      ? isSessionForStore(initialSession, storeSlug ?? null)
      : false,
  )
  const [canAccessAdmin, setCanAccessAdmin] = useState(
    initialSession?.canAccessAdmin ?? false,
  )
  const [adminStoreSlug, setAdminStoreSlug] = useState<string | null>(
    initialSession?.storeSlug ?? null,
  )
  const cartCount = getCartCount()

  const resolvedStoreSlug = storeSlug ?? defaultStoreSlug
  const displayName = getStoreDisplayName(storeName, resolvedStoreSlug)
  const storeScopedAccountBasePath = storeSlug
    ? storefrontPath(storeSlug, "/account")
    : "/account"
  const isOnCurrentStoreAccountPage =
    pathname === storeScopedAccountBasePath ||
    pathname.startsWith(`${storeScopedAccountBasePath}/`)

  const navigation = useMemo(
    () => [
      {
        name: "Collection",
        href: resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/products") : "/products",
      },
      {
        name: "Categories",
        href: resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/categories") : "/categories",
      },
      {
        name: "Brands",
        href: resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/brands") : "/brands",
      },
    ],
    [resolvedStoreSlug],
  )

  const homeHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug) : "/"
  const productsHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/products") : "/products"
  const cartHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/cart") : "/cart"
  const accountHref = isOnCurrentStoreAccountPage
    ? storeScopedAccountBasePath
    : isAuthenticated
      ? resolvedStoreSlug
        ? storefrontPath(resolvedStoreSlug, "/account")
        : "/account"
    : resolvedStoreSlug
      ? storefrontPath(resolvedStoreSlug, "/login")
      : "/auth/login"
  const canAccessCurrentStoreAdmin =
    canAccessAdmin &&
    (storeSlug
      ? adminStoreSlug?.toLowerCase() === storeSlug.toLowerCase()
      : true)
  const adminHref = adminStoreSlug
    ? storefrontPath(adminStoreSlug, "/admin")
    : "/admin"

  useEffect(() => {
    const handleScroll = () => {
      setScrolled(window.scrollY > 50)
    }

    window.addEventListener("scroll", handleScroll)
    return () => window.removeEventListener("scroll", handleScroll)
  }, [])

  useEffect(() => {
    let isCancelled = false

    async function loadAuthSession() {
      try {
        const session = await getAuthSession()

        if (!isCancelled) {
          setIsAuthenticated(isSessionForStore(session, storeSlug ?? null))
          setCanAccessAdmin(session.canAccessAdmin)
          setAdminStoreSlug(session.storeSlug)
        }
      } catch {
        if (!isCancelled) {
          setIsAuthenticated(false)
          setCanAccessAdmin(false)
          setAdminStoreSlug(null)
        }
      }
    }

    void loadAuthSession()

    return () => {
      isCancelled = true
    }
  }, [resolvedStoreSlug, storeSlug])

  return (
    <header
      className={cn(
        "fixed top-0 left-0 right-0 z-50 transition-all duration-500",
        scrolled
          ? "bg-background/98 backdrop-blur-md border-b border-border py-4"
          : "bg-transparent py-6",
      )}
    >
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-6 lg:px-8">
        <Button
          variant="ghost"
          size="icon"
          className="lg:hidden hover:bg-transparent"
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
        >
          {mobileMenuOpen ? (
            <X className="h-5 w-5" strokeWidth={1} />
          ) : (
            <Menu className="h-5 w-5" strokeWidth={1} />
          )}
          <span className="sr-only">Menu</span>
        </Button>

        <div className="hidden lg:flex lg:items-center lg:gap-x-12">
          {navigation.map((item) => (
            <Link
              key={item.name}
              href={item.href}
              className="premium-link text-[11px] font-normal uppercase tracking-[0.25em] text-foreground/80 transition-colors hover:text-foreground"
            >
              {item.name}
            </Link>
          ))}
        </div>

        <Link href={homeHref} className="absolute left-1/2 -translate-x-1/2">
          <span className="font-serif text-2xl font-light tracking-[0.4em] text-foreground lg:text-3xl">
            {displayName}
          </span>
        </Link>

        <div className="flex items-center gap-4">
          <Link href={productsHref}>
            <Button variant="ghost" size="icon" className="hidden md:flex hover:bg-transparent">
              <Search className="h-5 w-5" strokeWidth={1} />
              <span className="sr-only">Search</span>
            </Button>
          </Link>

          <Link href={accountHref}>
            <Button variant="ghost" size="icon" className="hidden md:flex hover:bg-transparent">
              <User className="h-5 w-5" strokeWidth={1} />
              <span className="sr-only">Account</span>
            </Button>
          </Link>

          {canAccessCurrentStoreAdmin ? (
            <Link href={adminHref}>
              <Button variant="ghost" size="icon" className="hidden md:flex hover:bg-transparent">
                <LayoutDashboard className="h-5 w-5" strokeWidth={1} />
                <span className="sr-only">Admin</span>
              </Button>
            </Link>
          ) : null}

          <Link href={cartHref}>
            <Button variant="ghost" size="icon" className="relative hover:bg-transparent">
              <ShoppingBag className="h-5 w-5" strokeWidth={1} />
              {cartCount > 0 ? (
                <span className="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center bg-foreground text-[9px] font-medium text-background">
                  {cartCount}
                </span>
              ) : null}
              <span className="sr-only">Cart</span>
            </Button>
          </Link>
        </div>
      </nav>

      <div
        className={cn(
          "lg:hidden fixed inset-0 top-[73px] bg-background transition-all duration-500",
          mobileMenuOpen ? "opacity-100 visible" : "opacity-0 invisible",
        )}
      >
        <div className="flex flex-col items-center justify-center h-full gap-8">
          {navigation.map((item, index) => (
            <Link
              key={item.name}
              href={item.href}
              className={cn(
                "text-2xl font-serif font-light tracking-[0.2em] text-foreground transition-all duration-500",
                mobileMenuOpen ? "opacity-100 translate-y-0" : "opacity-0 translate-y-4",
              )}
              style={{ transitionDelay: `${index * 100}ms` }}
              onClick={() => setMobileMenuOpen(false)}
            >
              {item.name}
            </Link>
          ))}

          {canAccessCurrentStoreAdmin ? (
            <Link
              href={adminHref}
              className={cn(
                "text-2xl font-serif font-light tracking-[0.2em] text-foreground transition-all duration-500",
                mobileMenuOpen ? "opacity-100 translate-y-0" : "opacity-0 translate-y-4",
              )}
              style={{ transitionDelay: `${navigation.length * 100}ms` }}
              onClick={() => setMobileMenuOpen(false)}
            >
              Admin
            </Link>
          ) : null}
        </div>
      </div>
    </header>
  )
}
