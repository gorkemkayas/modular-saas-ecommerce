'use client';

import { useState } from 'react';
import Link from 'next/link';
import { ChevronRight, Package } from 'lucide-react';
import { Header } from '@/components/header';
import { Footer } from '@/components/footer';

export default function CategoriesPage() {
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);

  const categories = [
    {
      slug: 'outerwear',
      name: 'Outerwear',
      description: 'Premium coats, jackets, and blazers',
      productCount: 24,
      subcategories: ['Coats', 'Jackets', 'Blazers', 'Vests'],
      featured: true,
    },
    {
      slug: 'tops',
      name: 'Tops',
      description: 'Essential and luxury tops',
      productCount: 45,
      subcategories: ['Shirts', 'Blouses', 'T-Shirts', 'Knitwear'],
    },
    {
      slug: 'bottoms',
      name: 'Bottoms',
      description: 'Trousers, skirts, and more',
      productCount: 38,
      subcategories: ['Trousers', 'Skirts', 'Shorts', 'Jeans'],
    },
    {
      slug: 'dresses',
      name: 'Dresses',
      description: 'Evening and casual dresses',
      productCount: 31,
      subcategories: ['Casual', 'Evening', 'Midi', 'Maxi'],
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
              <span className="text-black font-light">All Categories</span>
            </nav>
            <h1 className="text-5xl font-light tracking-tight mb-4">Collections</h1>
            <p className="text-gray-600 text-lg leading-relaxed max-w-2xl">
              Discover our carefully curated collections of premium apparel and accessories.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {categories.map((category) => (
              <Link
                key={category.slug}
                href={`/categories/${category.slug}`}
                className="group"
              >
                <div className="border border-black p-8 hover:bg-black hover:text-white transition-colors duration-300">
                  <div className="flex items-start justify-between mb-6">
                    <div>
                      <h2 className="text-3xl font-light tracking-tight mb-2">
                        {category.name}
                      </h2>
                      <p className="text-sm opacity-75">{category.description}</p>
                    </div>
                    {category.featured && (
                      <span className="text-xs font-light tracking-widest uppercase px-3 py-1 border border-current">
                        Featured
                      </span>
                    )}
                  </div>

                  <div className="mb-6 pb-6 border-t border-current border-opacity-20">
                    <p className="text-sm font-light">{category.productCount} products</p>
                  </div>

                  <div className="flex flex-wrap gap-2 mb-6">
                    {category.subcategories.map((sub) => (
                      <span
                        key={sub}
                        className="text-xs opacity-60 font-light"
                      >
                        {sub}
                        {category.subcategories.indexOf(sub) < category.subcategories.length - 1 && (
                          <span className="mx-2">•</span>
                        )}
                      </span>
                    ))}
                  </div>

                  <div className="flex items-center gap-2 text-sm uppercase tracking-widest font-light group-hover:translate-x-1 transition-transform">
                    Explore
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
