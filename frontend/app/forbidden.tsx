'use client';

import Link from 'next/link';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';

export default function ForbiddenPage() {
  return (
    <div className="flex flex-col min-h-screen bg-white">
      <Header />

      <main className="flex-1 flex items-center justify-center pt-32">
        <div className="max-w-2xl mx-auto px-8 py-16 text-center">
          <div className="mb-8">
            <div className="text-7xl font-light mb-6">403</div>
            <h1 className="text-4xl font-light tracking-tight mb-4">Access Denied</h1>
            <p className="text-lg text-gray-600 leading-relaxed">
              You don&apos;t have permission to access this resource.
            </p>
          </div>

          <div className="flex gap-4 justify-center">
            <Link
              href="/"
              className="px-8 py-4 border border-black hover:bg-black hover:text-white transition-colors font-light tracking-widest uppercase text-sm"
            >
              Return Home
            </Link>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
