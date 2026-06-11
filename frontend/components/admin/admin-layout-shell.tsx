"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import {
  LayoutDashboard,
  Package,
  FolderTree,
  Tags,
  Palette,
  Boxes,
  ArrowLeftRight,
  DollarSign,
  CreditCard,
  KeyRound,
  Truck,
  Users,
  Bell,
  Settings,
  ChevronLeft,
  Menu,
  LogOut,
  Store,
  Sparkles,
  type LucideIcon,
} from "lucide-react"
import { useState } from "react"
import { resolveAdminBasePath, resolveAdminStoreSlug } from "@/lib/admin-path"
import { getStoreDisplayName } from "@/lib/store-branding"
import { cn } from "@/lib/utils"

type NavigationDivider = {
  type: "divider"
  label: string
}

type NavigationLink = {
  type: "link"
  name: string
  href: string
  icon: LucideIcon
}

const navigation: Array<NavigationDivider | NavigationLink> = [
  { type: "link", name: "Dashboard", href: "", icon: LayoutDashboard },
  { type: "link", name: "Store Settings", href: "/store-settings", icon: Store },
  { type: "link", name: "Subscription", href: "/subscription", icon: Sparkles },
  { type: "divider", label: "Catalog" },
  { type: "link", name: "Products", href: "/products", icon: Package },
  { type: "link", name: "Categories", href: "/categories", icon: FolderTree },
  { type: "link", name: "Brands", href: "/brands", icon: Tags },
  { type: "link", name: "Attributes", href: "/attributes", icon: Palette },
  { type: "divider", label: "Inventory" },
  { type: "link", name: "Stock Management", href: "/inventory", icon: Boxes },
  { type: "link", name: "Stock Movements", href: "/stock-movements", icon: ArrowLeftRight },
  { type: "divider", label: "Sales" },
  { type: "link", name: "Price Lists", href: "/prices", icon: DollarSign },
  { type: "link", name: "Payments", href: "/payments", icon: CreditCard },
  { type: "link", name: "Payment Settings", href: "/payment-settings", icon: KeyRound },
  { type: "link", name: "Shipments", href: "/shipments", icon: Truck },
  { type: "divider", label: "Customers" },
  { type: "link", name: "Customers", href: "/customers", icon: Users },
  { type: "link", name: "Notifications", href: "/notifications", icon: Bell },
]

export function AdminLayoutShell({
  children,
  storeName,
}: {
  children: React.ReactNode
  storeName?: string | null
}) {
  const pathname = usePathname()
  const [collapsed, setCollapsed] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const basePath = resolveAdminBasePath(pathname)
  const storeSlug = resolveAdminStoreSlug(pathname)
  const storeHref = storeSlug ? `/${storeSlug}` : "/"
  const displayName = getStoreDisplayName(storeName, storeSlug, "Admin")

  return (
    <div className="min-h-screen bg-background">
      <div className="fixed left-0 right-0 top-0 z-50 flex h-16 items-center justify-between bg-primary px-4 text-primary-foreground lg:hidden">
        <button onClick={() => setMobileOpen(true)}>
          <Menu className="h-6 w-6" strokeWidth={1.5} />
        </button>
        <Link href={basePath} className="mx-4 truncate text-center text-sm font-light tracking-[0.22em] sm:text-lg sm:tracking-[0.3em]">
          {displayName}
        </Link>
        <div className="w-6" />
      </div>

      {mobileOpen && (
        <div
          className="lg:hidden fixed inset-0 bg-foreground/50 z-50"
          onClick={() => setMobileOpen(false)}
        />
      )}

      <aside
        className={cn(
          "fixed top-0 left-0 z-50 flex h-full max-w-[85vw] flex-col bg-primary text-primary-foreground transition-all duration-300",
          collapsed ? "w-20" : "w-64",
          mobileOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0",
        )}
      >
        <div className="flex h-16 items-center justify-between border-b border-primary-foreground/10 px-4 sm:px-6">
          {!collapsed && (
            <Link href={basePath} className="text-lg tracking-[0.3em] font-light">
              {displayName}
            </Link>
          )}
          <button
            onClick={() => {
              setCollapsed(!collapsed)
              setMobileOpen(false)
            }}
            className="p-1 hover:bg-primary-foreground/10 transition-colors"
          >
            <ChevronLeft className={cn("h-5 w-5 transition-transform", collapsed && "rotate-180")} strokeWidth={1.5} />
          </button>
        </div>

        <div className={cn("px-6 py-4 border-b border-primary-foreground/10", collapsed && "px-4 py-3")}>
          {!collapsed ? (
            <p className="text-xs tracking-[0.2em] text-primary-foreground/60 uppercase">Admin Panel</p>
          ) : (
            <Settings className="h-5 w-5 mx-auto text-primary-foreground/60" strokeWidth={1.5} />
          )}
        </div>

        <nav className="flex-1 overflow-y-auto py-4">
          {navigation.map((item, index) => {
            if (item.type === "divider") {
              return (
                <div key={index} className={cn("mt-4 mb-2", collapsed ? "px-4" : "px-6")}>
                  {!collapsed ? (
                    <p className="text-[10px] tracking-[0.2em] text-primary-foreground/40 uppercase">
                      {item.label}
                    </p>
                  ) : (
                    <div className="border-t border-primary-foreground/10" />
                  )}
                </div>
              )
            }

            const Icon = item.icon
            const href = `${basePath}${item.href}`
            const isActive =
              pathname === href ||
              (href !== basePath && pathname?.startsWith(href))

            return (
              <Link
                key={href}
                href={href}
                onClick={() => setMobileOpen(false)}
                className={cn(
                  "flex items-center gap-3 transition-colors",
                  collapsed ? "px-4 py-3 justify-center" : "px-6 py-2.5",
                  isActive
                    ? "bg-primary-foreground/10 text-primary-foreground"
                    : "text-primary-foreground/60 hover:text-primary-foreground hover:bg-primary-foreground/5",
                )}
              >
                <Icon className="h-5 w-5 shrink-0" strokeWidth={1.5} />
                {!collapsed && <span className="text-sm tracking-wide">{item.name}</span>}
              </Link>
            )
          })}
        </nav>

        <div className="border-t border-primary-foreground/10 p-4">
          <Link
            href={storeHref}
            className={cn(
              "flex items-center gap-3 text-primary-foreground/60 hover:text-primary-foreground transition-colors",
              collapsed && "justify-center",
            )}
          >
            <LogOut className="h-5 w-5" strokeWidth={1.5} />
            {!collapsed && <span className="text-sm">Back to Store</span>}
          </Link>
        </div>
      </aside>

      <main
        className={cn(
          "min-h-screen overflow-x-hidden pt-16 transition-all duration-300 lg:pt-0",
          collapsed ? "lg:pl-20" : "lg:pl-64",
        )}
      >
        <div className="p-4 sm:p-6 lg:p-8">{children}</div>
      </main>
    </div>
  )
}
