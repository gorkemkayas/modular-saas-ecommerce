'use client';

import Link from 'next/link';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';

export default function ErrorPage() {
  return (
    <div className="flex flex-col min-h-screen bg-white">
      <Header />

      <main className="flex-1 flex items-center justify-center pt-32">
        <div className="max-w-2xl mx-auto px-8 py-16 text-center">
          <div className="mb-8">
            <div className="text-7xl font-light mb-6">500</div>
            <h1 className="text-4xl font-light tracking-tight mb-4">Something Went Wrong</h1>
            <p className="text-lg text-gray-600 leading-relaxed mb-8">
              We encountered an unexpected error. Our team has been notified and we&apos;re working to fix it.
            </p>
          </div>

          <div className="flex gap-4 justify-center">
            <Link
              href="/"
              className="px-8 py-4 border border-black hover:bg-black hover:text-white transition-colors font-light tracking-widest uppercase text-sm"
            >
              Return Home
            </Link>
            <Link
              href="/contact"
              className="px-8 py-4 bg-black text-white hover:bg-gray-800 transition-colors font-light tracking-widest uppercase text-sm"
            >
              Contact Support
            </Link>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
