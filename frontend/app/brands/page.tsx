'use client';

import { useState } from 'react';
import Link from 'next/link';
import { ChevronRight, Star } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';

export default function BrandsPage() {
  const brands = [
    {
      slug: 'kayas-premium',
      name: 'KAYAS Premium',
      description: 'Our signature luxury collection',
      logo: '🏛️',
      productCount: 156,
      featured: true,
      established: '2020',
    },
    {
      slug: 'kayas-essentials',
      name: 'KAYAS Essentials',
      description: 'Timeless basics and staples',
      logo: '✨',
      productCount: 89,
      featured: true,
      established: '2021',
    },
    {
      slug: 'kayas-studio',
      name: 'KAYAS Studio',
      description: 'Contemporary and experimental',
      logo: '🎨',
      productCount: 67,
      established: '2022',
    },
    {
      slug: 'kayas-heritage',
      name: 'KAYAS Heritage',
      description: 'Vintage and archive pieces',
      logo: '🕰️',
      productCount: 43,
      established: '2023',
    },
  ];

  return (
    <div className="flex flex-col min-h-screen bg-white">
      <Header />

      <main className="flex-1 pt-32">
        <div className="max-w-7xl mx-auto px-8 py-16">
          <div className="mb-12">
            <nav className="flex items-center gap-2 text-sm mb-8 uppercase tracking-widest">
              <Link href="/" className="text-gray-500 hover:text-black transition-colors">
                Home
              </Link>
              <ChevronRight className="w-4 h-4 text-gray-400" />
              <span className="text-black font-light">Brands</span>
            </nav>
            <h1 className="text-5xl font-light tracking-tight mb-4">Our Brands</h1>
            <p className="text-gray-600 text-lg leading-relaxed max-w-2xl">
              Explore our diverse portfolio of carefully crafted brand collections.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {brands.map((brand) => (
              <Link
                key={brand.slug}
                href={`/brands/${brand.slug}`}
                className="group"
              >
                <div className="border border-black p-8 hover:bg-black hover:text-white transition-colors duration-300">
                  <div className="flex items-start justify-between mb-6">
                    <div className="text-5xl mb-4">{brand.logo}</div>
                    {brand.featured && (
                      <Star className="w-5 h-5 fill-current" strokeWidth={1} />
                    )}
                  </div>

                  <h2 className="text-3xl font-light tracking-tight mb-2">{brand.name}</h2>
                  <p className="text-sm opacity-75 mb-6">{brand.description}</p>

                  <div className="pb-6 border-t border-current border-opacity-20 pt-6 mb-6 space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="opacity-60 font-light">Products</span>
                      <span className="font-light">{brand.productCount}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="opacity-60 font-light">Established</span>
                      <span className="font-light">{brand.established}</span>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 text-sm uppercase tracking-widest font-light group-hover:translate-x-1 transition-transform">
                    Explore Brand
                    <ChevronRight className="w-4 h-4" />
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
