'use client';

import Link from 'next/link';

export default function ReturnPolicyPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="border-b border-black/10">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <h1 className="text-4xl sm:text-5xl font-light tracking-wider mb-4">
            RETURN POLICY
          </h1>
          <p className="text-gray-600 font-light">Last updated: May 2024</p>
        </div>
      </section>

      {/* Content */}
      <section className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="space-y-12">
          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">RETURN WINDOW</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              You have 30 days from the date of purchase to return items for a refund or exchange. Items must be in original condition with all tags attached. Returns initiated after 30 days will not be accepted.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">CONDITION REQUIREMENTS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Items must be unworn, unwashed, and undamaged. Original tags must be attached and intact. For acceptance, items must show no signs of wear or use.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">HOW TO RETURN</h2>
            <ol className="space-y-3 text-gray-600">
              <li><strong>1. Initiate Return:</strong> Log into your account and select &quot;Return&quot; on the order.</li>
              <li><strong>2. Pack Your Items:</strong> Securely package the item(s) in original packaging.</li>
              <li><strong>3. Print Label:</strong> Download and print the return shipping label from your account.</li>
              <li><strong>4. Ship:</strong> Drop off at the designated carrier location.</li>
              <li><strong>5. Track:</strong> Monitor your return status in your account.</li>
            </ol>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">SHIPPING COSTS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We provide prepaid return labels for all eligible returns. If you choose an alternative return method, you are responsible for return shipping costs.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">REFUND PROCESSING</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Once we receive and inspect your return, refunds are processed within 5-7 business days. Refunds are issued to the original payment method. Please allow 3-5 business days for the amount to appear in your account.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">EXCHANGES</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              If you would like to exchange an item for a different size or color, we will process the exchange free of shipping charges. Simply note this in your return request.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">SALE ITEMS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Final sale items marked as such during purchase cannot be returned or exchanged. These items are offered at a special price and are non-refundable.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">DEFECTIVE ITEMS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              If you receive a defective or damaged item, we will replace it or provide a full refund regardless of the time elapsed. Please contact us within 14 days of receipt with photos and details.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">INTERNATIONAL RETURNS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              International customers are responsible for return shipping costs. We cannot provide prepaid labels for international returns. Once received, refunds will be processed according to the standard timeline.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">QUESTIONS?</h2>
            <p className="text-gray-600 leading-relaxed">
              Contact our customer service team at support@kayas.com or visit the Help section for more information.
            </p>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-black text-white border-t border-black">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="flex justify-between items-center">
            <p className="text-gray-300">Questions about returns?</p>
            <Link href="/help" className="px-8 h-12 border border-white flex items-center hover:bg-white/5 transition-colors font-medium tracking-wider">
              VIEW FAQ
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
