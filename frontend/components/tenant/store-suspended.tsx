import Link from 'next/link'
import { AlertTriangle } from 'lucide-react'
import { storefrontPath, withQuery } from '@/lib/config'
import type { Tenant } from '@/lib/tenant'

interface StoreSuspendedProps {
  tenant: Tenant
}

export function StoreSuspended({ tenant }: StoreSuspendedProps) {
  const adminLoginHref = withQuery("/auth/login", {
    intent: "admin",
    storeSlug: tenant.slug,
    next: storefrontPath(tenant.slug, "/admin"),
  })

  return (
    <div className="min-h-screen bg-white flex flex-col">
      {/* Minimal Header */}
      <header className="border-b border-gray-100">
        <div className="max-w-7xl mx-auto px-6 py-3">
          <span className="text-lg font-light tracking-[0.3em] text-gray-400">
            {tenant.name}
          </span>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 flex items-center justify-center px-6 py-6">
        <div className="max-w-2xl w-full text-center">
          {/* Icon */}
          <div className="mb-6">
            <div className="inline-flex items-center justify-center w-20 h-20 border border-amber-200 bg-amber-50 rounded-full mb-5">
              <AlertTriangle className="w-8 h-8 text-amber-500" strokeWidth={1} />
            </div>
          </div>

          {/* Content */}
          <h1 className="text-3xl md:text-4xl font-light tracking-tight text-black mb-4">
            Store Suspended
          </h1>
          
          <p className="text-sm md:text-base text-gray-600 leading-relaxed mb-3 max-w-lg mx-auto">
            This store has been temporarily suspended and is not currently accepting orders.
          </p>
          
          <p className="text-sm text-gray-500 mb-6">
            If you believe this is an error, please contact the store owner or our support team.
          </p>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Link
              href={adminLoginHref}
              className="inline-flex items-center justify-center px-8 py-3 bg-black text-white text-[11px] font-light tracking-[0.2em] uppercase hover:bg-gray-900 transition-colors"
            >
              Admin Sign In
            </Link>
            <Link
              href="/"
              className="inline-flex items-center justify-center px-8 py-3 border border-black text-black text-[11px] font-light tracking-[0.2em] uppercase hover:bg-black hover:text-white transition-colors"
            >
              Explore Other Stores
            </Link>
            <Link
              href="/contact"
              className="inline-flex items-center justify-center px-8 py-3 border border-black text-black text-[11px] font-light tracking-[0.2em] uppercase hover:bg-black hover:text-white transition-colors"
            >
              Contact Support
            </Link>
          </div>
        </div>
      </main>

      {/* Minimal Footer */}
      <footer className="border-t border-gray-100 py-4">
        <div className="max-w-7xl mx-auto px-6 text-center">
          <p className="text-xs text-gray-500">
            &copy; {new Date().getFullYear()} {tenant.name}. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  )
}
