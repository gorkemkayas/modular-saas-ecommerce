"use client"

import Link from 'next/link'
import { ShieldX, ArrowLeft, LogIn } from 'lucide-react'
import { storefrontPath } from '@/lib/config'
import { getStoreDisplayName } from '@/lib/store-branding'

interface UnauthorizedProps {
  storeSlug?: string
  storeName?: string | null
}

export function Unauthorized({ storeSlug, storeName }: UnauthorizedProps) {
  const homeHref = storeSlug ? storefrontPath(storeSlug) : "/"
  const loginHref = storeSlug ? storefrontPath(storeSlug, "/login") : "/auth/login"
  const displayName = getStoreDisplayName(storeName, storeSlug)

  return (
    <div className="min-h-screen bg-white flex flex-col">
      <header className="border-b border-neutral-100">
        <div className="max-w-7xl mx-auto px-6 py-6">
          <Link href={homeHref} className="text-2xl font-light tracking-[0.3em] text-black">
            {displayName}
          </Link>
        </div>
      </header>

      <main className="flex-1 flex items-center justify-center px-6 py-20">
        <div className="max-w-xl w-full text-center">
          <div className="mb-12">
            <div className="inline-flex items-center justify-center w-28 h-28 border border-neutral-200 mb-8">
              <ShieldX className="w-12 h-12 text-neutral-400" strokeWidth={1} />
            </div>
          </div>

          <p className="text-sm text-neutral-500 uppercase tracking-[0.2em] mb-4">
            Access Denied
          </p>

          <h1 className="text-4xl md:text-5xl lg:text-6xl font-light tracking-tight text-black mb-8">
            Unauthorized
          </h1>

          <p className="text-lg text-neutral-600 mb-4">
            You don&apos;t have permission to access this page.
          </p>

          <p className="text-base text-neutral-500 mb-12 max-w-md mx-auto">
            Please sign in with an account that has the required permissions,
            or contact support if you believe this is an error.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center mb-16">
            <Link
              href={loginHref}
              className="inline-flex items-center justify-center gap-2 px-10 py-4 bg-black text-white text-sm font-light tracking-[0.2em] uppercase hover:bg-neutral-900 transition-colors"
            >
              <LogIn className="w-4 h-4" strokeWidth={1.5} />
              Sign In
            </Link>
            <button
              onClick={() => window.history.back()}
              className="inline-flex items-center justify-center gap-2 px-10 py-4 border border-black text-black text-sm font-light tracking-[0.2em] uppercase hover:bg-black hover:text-white transition-colors"
            >
              <ArrowLeft className="w-4 h-4" strokeWidth={1.5} />
              Go Back
            </button>
          </div>

          <div className="flex items-center justify-center gap-6 text-sm">
            <Link
              href="/help"
              className="text-neutral-500 hover:text-black transition-colors underline-offset-4 hover:underline"
            >
              Help Center
            </Link>
            <span className="text-neutral-300">|</span>
            <Link
              href="/contact"
              className="text-neutral-500 hover:text-black transition-colors underline-offset-4 hover:underline"
            >
              Contact Support
            </Link>
          </div>
        </div>
      </main>

      <footer className="border-t border-neutral-100 py-8">
        <div className="max-w-7xl mx-auto px-6 text-center">
          <p className="text-sm text-neutral-500">
            &copy; {new Date().getFullYear()} {displayName}. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  )
}
