'use client';

import Link from 'next/link';

export default function ShippingPolicyPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="border-b border-black/10">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <h1 className="text-4xl sm:text-5xl font-light tracking-wider mb-4">
            SHIPPING POLICY
          </h1>
          <p className="text-gray-600 font-light">Last updated: May 2024</p>
        </div>
      </section>

      {/* Content */}
      <section className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="space-y-12">
          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">SHIPPING METHODS</h2>
            <div className="space-y-4 text-gray-600">
              <div>
                <h3 className="font-medium mb-2">Standard Shipping</h3>
                <p>Delivery within 5-7 business days. Free on orders over $100.</p>
              </div>
              <div>
                <h3 className="font-medium mb-2">Express Shipping</h3>
                <p>Delivery within 2-3 business days. $15 flat rate.</p>
              </div>
              <div>
                <h3 className="font-medium mb-2">Overnight Shipping</h3>
                <p>Delivery next business day. $35 flat rate.</p>
              </div>
            </div>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">PROCESSING TIME</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              All orders are processed within 24 hours during business days (Monday - Friday, 9 AM - 6 PM EST). Orders placed on weekends or holidays will be processed on the next business day.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">DELIVERY AREAS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We currently ship to all addresses within the continental United States, Canada, and select international destinations. International shipping rates and times vary by location.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">TRACKING YOUR ORDER</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Once your order ships, you will receive a tracking number via email. You can use this number to track your shipment in real-time through our website or the carrier&apos;s website.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">PACKAGING</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We take pride in our presentation and package all items with care. Each order is carefully wrapped and shipped in premium packaging to ensure your items arrive in perfect condition.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">DAMAGED OR LOST SHIPMENTS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              If your package arrives damaged or goes missing, please contact us immediately at support@kayas.com with photos and tracking information. We will work with the carrier to resolve the issue and replace or refund your order.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">CUSTOMS & DUTIES</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              For international orders, the customer is responsible for any applicable customs duties, taxes, or additional fees imposed by their country of destination.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">HOLIDAY & SPECIAL CLOSURES</h2>
            <p className="text-gray-600 leading-relaxed">
              During holiday seasons or special events, processing and delivery times may be extended. We will notify you of any delays when you receive your shipping confirmation.
            </p>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-black text-white border-t border-black">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="flex justify-between items-center">
            <p className="text-gray-300">Ready to shop?</p>
            <Link href="/products" className="px-8 h-12 border border-white flex items-center hover:bg-white/5 transition-colors font-medium tracking-wider">
              BROWSE PRODUCTS
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
