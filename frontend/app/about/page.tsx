'use client';

import Link from 'next/link';

export default function AboutPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="border-b border-black/10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <h1 className="text-5xl sm:text-6xl font-light tracking-wider mb-6">
            ABOUT KAYAS
          </h1>
          <p className="text-lg text-gray-600 font-light max-w-2xl">
            Crafting premium fashion with timeless elegance and modern sensibility since our founding.
          </p>
        </div>
      </section>

      {/* Brand Story */}
      <section className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20 border-b border-black/10">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-16 items-center">
          <div className="h-96 bg-black/5"></div>
          <div>
            <h2 className="text-3xl font-light tracking-wider mb-6">OUR STORY</h2>
            <p className="text-gray-600 mb-4 leading-relaxed">
              KAYAS was founded with a simple vision: to create clothing that transcends trends and embodies timeless elegance. Our name draws inspiration from the Turkish word for &quot;source,&quot; reflecting our commitment to being the wellspring of quality, design, and craftsmanship.
            </p>
            <p className="text-gray-600 mb-4 leading-relaxed">
              From our atelier to your wardrobe, every piece tells a story of meticulous attention to detail, premium materials, and uncompromising design principles. We believe that true luxury is about simplicity, quality, and the perfect balance between form and function.
            </p>
            <p className="text-gray-600 leading-relaxed">
              Each collection is thoughtfully curated to celebrate the art of dressing well, whether for everyday moments or special occasions.
            </p>
          </div>
        </div>
      </section>

      {/* Values */}
      <section className="bg-black text-white py-20 border-b border-black">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-light tracking-wider mb-16">OUR VALUES</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
            <div>
              <h3 className="text-xl font-light tracking-wider mb-4">QUALITY</h3>
              <p className="text-gray-300 leading-relaxed">
                We source only the finest materials and employ master craftsmen to ensure every piece meets our exacting standards.
              </p>
            </div>
            <div>
              <h3 className="text-xl font-light tracking-wider mb-4">DESIGN</h3>
              <p className="text-gray-300 leading-relaxed">
                Our design philosophy celebrates minimalism, elegance, and timeless appeal over fleeting trends.
              </p>
            </div>
            <div>
              <h3 className="text-xl font-light tracking-wider mb-4">SUSTAINABILITY</h3>
              <p className="text-gray-300 leading-relaxed">
                We are committed to ethical production practices and environmental responsibility in all our operations.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Team */}
      <section className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20 border-b border-black/10">
        <h2 className="text-3xl font-light tracking-wider mb-16">LEADERSHIP TEAM</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
          {[
            { name: 'Sophia Laurent', title: 'Creative Director', bio: 'Visionary designer with 20+ years in luxury fashion' },
            { name: 'Marcus Chen', title: 'CEO', bio: 'Building sustainable fashion businesses worldwide' },
            { name: 'Elena Rodriguez', title: 'Head of Operations', bio: 'Dedicated to ethical manufacturing and quality control' },
          ].map((member) => (
            <div key={member.name}>
              <div className="h-64 bg-black/5 mb-6"></div>
              <h3 className="text-lg font-medium tracking-wider mb-2">{member.name}</h3>
              <p className="text-sm text-gray-500 uppercase tracking-wider mb-3">{member.title}</p>
              <p className="text-gray-600 text-sm leading-relaxed">{member.bio}</p>
            </div>
          ))}
        </div>
      </section>

      {/* CTA */}
      <section className="bg-white border-t border-black/10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-light tracking-wider mb-4">EXPLORE OUR COLLECTIONS</h2>
            <p className="text-gray-600 mb-8">Discover timeless pieces crafted with care</p>
            <Link href="/products" className="inline-flex items-center justify-center h-14 px-8 bg-black text-white font-medium tracking-wider hover:bg-black/90 transition-colors">
              SHOP NOW
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
