"use client"

import Link from 'next/link'
import { Store, ArrowRight } from 'lucide-react'

interface StoreNotFoundProps {
  slug?: string
}

export function StoreNotFound({ slug }: StoreNotFoundProps) {
  return (
    <div className="min-h-screen bg-white flex flex-col">
      {/* Minimal Header */}
      <header className="border-b border-neutral-100">
        <div className="max-w-7xl mx-auto px-6 py-3">
          <Link href="/" className="text-lg font-light tracking-[0.25em] text-black">
            STOREFRONT
          </Link>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 flex items-center justify-center px-6 py-6">
        <div className="max-w-2xl w-full text-center">
          {/* Icon */}
          <div className="mb-4">
            <div className="inline-flex items-center justify-center w-16 h-16 border border-neutral-200 mb-4">
              <Store className="w-8 h-8 text-neutral-400" strokeWidth={1} />
            </div>
          </div>

          {/* Content */}
          <p className="text-[11px] text-neutral-500 uppercase tracking-[0.2em] mb-2">
            Store Not Found
          </p>
          
          <h1 className="text-3xl md:text-4xl font-light tracking-tight text-black mb-4">
            This store doesn&apos;t exist
          </h1>
          
          {slug && (
            <p className="text-sm md:text-base text-neutral-600 mb-2">
              We couldn&apos;t find a store with the address{' '}
              <span className="font-medium text-black">&quot;/{slug}&quot;</span>
            </p>
          )}
          
          <p className="text-sm text-neutral-500 mb-5 max-w-md mx-auto leading-relaxed">
            The store may have been removed, or the URL might be incorrect. 
            Try searching for what you&apos;re looking for.
          </p>

          {/* Search Box */}
          <div className="max-w-md mx-auto mb-5">
            <div className="relative">
              <input
                type="text"
                placeholder="Search for stores..."
                className="w-full border border-neutral-200 py-2.5 pl-4 pr-4 text-sm focus:outline-none focus:border-black transition-colors"
              />
            </div>
          </div>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-3 justify-center mb-5">
            <Link
              href="/"
              className="inline-flex items-center justify-center gap-2 px-7 py-2.5 bg-black text-white text-[11px] font-light tracking-[0.2em] uppercase hover:bg-neutral-900 transition-colors"
            >
              Browse All Stores
              <ArrowRight className="w-4 h-4" strokeWidth={1.5} />
            </Link>
            <Link
              href="/contact"
              className="inline-flex items-center justify-center px-7 py-2.5 border border-black text-black text-[11px] font-light tracking-[0.2em] uppercase hover:bg-black hover:text-white transition-colors"
            >
              Contact Support
            </Link>
          </div>

          {/* Divider */}
          <div className="flex items-center gap-3 mb-4">
            <div className="flex-1 h-px bg-neutral-200" />
            <span className="text-[10px] text-neutral-400 uppercase tracking-[0.2em]">Popular Stores</span>
            <div className="flex-1 h-px bg-neutral-200" />
          </div>

          {/* Popular Stores */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 max-w-lg mx-auto">
            <Link
              href="/kayas"
              className="group flex items-center gap-3 p-2.5 border border-neutral-200 hover:border-black transition-colors"
            >
              <div className="w-9 h-9 bg-black flex items-center justify-center text-white text-[10px] font-light tracking-wider">
                FS
              </div>
              <div className="text-left">
                <p className="text-sm font-medium text-black group-hover:underline">Flagship Store</p>
                <p className="text-xs text-neutral-500">Premium Fashion</p>
              </div>
            </Link>
            <Link
              href="/luxe-boutique"
              className="group flex items-center gap-3 p-2.5 border border-neutral-200 hover:border-black transition-colors"
            >
              <div className="w-9 h-9 bg-neutral-900 flex items-center justify-center text-white text-[10px] font-light tracking-wider">
                LB
              </div>
              <div className="text-left">
                <p className="text-sm font-medium text-black group-hover:underline">Luxe Boutique</p>
                <p className="text-xs text-neutral-500">Designer Collections</p>
              </div>
            </Link>
          </div>
        </div>
      </main>

      {/* Minimal Footer */}
      <footer className="border-t border-neutral-100 py-3">
        <div className="max-w-7xl mx-auto px-6 flex flex-col sm:flex-row items-center justify-between gap-3">
          <p className="text-xs text-neutral-500">
            &copy; {new Date().getFullYear()} Storefront Marketplace
          </p>
          <div className="flex items-center gap-5">
            <Link href="/help" className="text-sm text-neutral-500 hover:text-black transition-colors">
              Help Center
            </Link>
            <Link href="/contact" className="text-sm text-neutral-500 hover:text-black transition-colors">
              Contact
            </Link>
          </div>
        </div>
      </footer>
    </div>
  )
}
