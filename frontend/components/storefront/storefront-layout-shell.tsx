"use client"

import type { ReactNode } from "react"
import { usePathname } from "next/navigation"

import { Footer } from "@/components/footer"
import { Header } from "@/components/header"
import type { AuthSessionResponse } from "@/lib/api/auth"

type StorefrontLayoutSessionSnapshot = Pick<
  AuthSessionResponse,
  "isAuthenticated" | "canAccessAdmin" | "storeSlug"
>

interface StorefrontLayoutShellProps {
  children: ReactNode
  storeSlug: string
  storeName?: string | null
  initialSession?: StorefrontLayoutSessionSnapshot
}

const AUTH_SEGMENTS = new Set(["login", "register", "forgot-password", "unauthorized"])

export function StorefrontLayoutShell({
  children,
  storeSlug,
  storeName,
  initialSession,
}: StorefrontLayoutShellProps) {
  const pathname = usePathname()
  const pathSegments = pathname.split("/").filter(Boolean)
  const lastSegment = pathSegments.at(-1)
  const isAuthPage = lastSegment ? AUTH_SEGMENTS.has(lastSegment) : false
  const isStoreHomePage = pathSegments.length === 1 && pathSegments[0] === storeSlug
  const isAdminPage = pathSegments[0] === storeSlug && pathSegments[1] === "admin"

  if (isAuthPage || isAdminPage) {
    return <>{children}</>
  }

  return (
    <>
      <Header
        storeSlug={storeSlug}
        storeName={storeName}
        initialSession={initialSession}
      />
      <main className={isStoreHomePage ? "min-h-screen" : "min-h-screen pt-24"}>
        {children}
      </main>
      <Footer storeSlug={storeSlug} storeName={storeName} />
    </>
  )
}
