'use client';

import Link from 'next/link';

export default function PrivacyPolicyPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="border-b border-black/10">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <h1 className="text-4xl sm:text-5xl font-light tracking-wider mb-4">
            PRIVACY POLICY
          </h1>
          <p className="text-gray-600 font-light">Last updated: May 2024</p>
        </div>
      </section>

      {/* Content */}
      <section className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="space-y-12">
          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">1. INTRODUCTION</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              KAYAS (&quot;Company,&quot; &quot;we,&quot; &quot;us,&quot; or &quot;our&quot;) operates the kayas.com website (the &quot;Site&quot;) and the KAYAS application (the &quot;App&quot;). This Privacy Policy explains how we collect, use, disclose, and otherwise handle your information.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">2. INFORMATION WE COLLECT</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              <strong>Account Information:</strong> When you create an account, we collect your name, email address, password, phone number, and billing/shipping addresses.
            </p>
            <p className="text-gray-600 leading-relaxed mb-4">
              <strong>Order Information:</strong> We collect details about your purchases, including items ordered, quantities, prices, and payment information.
            </p>
            <p className="text-gray-600 leading-relaxed mb-4">
              <strong>Communication Data:</strong> When you contact us, we collect your messages, feedback, and any attachments you provide.
            </p>
            <p className="text-gray-600 leading-relaxed">
              <strong>Automatic Information:</strong> We automatically collect IP address, browser type, device information, and browsing behavior through cookies and analytics.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">3. HOW WE USE YOUR INFORMATION</h2>
            <ul className="space-y-3 text-gray-600">
              <li>• To process and fulfill your orders</li>
              <li>• To send transactional emails and order updates</li>
              <li>• To improve our website and services</li>
              <li>• To personalize your shopping experience</li>
              <li>• To detect and prevent fraud</li>
              <li>• To comply with legal obligations</li>
              <li>• To send marketing communications (with your consent)</li>
            </ul>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">4. HOW WE PROTECT YOUR INFORMATION</h2>
            <p className="text-gray-600 leading-relaxed">
              We implement appropriate technical and organizational security measures to protect your personal information against unauthorized access, alteration, disclosure, or destruction. All payment information is encrypted and processed through secure payment gateways.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">5. COOKIES AND TRACKING</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We use cookies and similar tracking technologies to enhance your experience, remember your preferences, and analyze site traffic. You can control cookie settings through your browser preferences.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">6. THIRD-PARTY SHARING</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              We may share your information with:
            </p>
            <ul className="space-y-3 text-gray-600">
              <li>• Payment processors for transaction processing</li>
              <li>• Shipping carriers for delivery</li>
              <li>• Marketing partners (with your consent)</li>
              <li>• Law enforcement when legally required</li>
            </ul>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">7. YOUR RIGHTS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              You have the right to:
            </p>
            <ul className="space-y-3 text-gray-600">
              <li>• Access your personal information</li>
              <li>• Correct inaccurate data</li>
              <li>• Request deletion of your data</li>
              <li>• Opt-out of marketing communications</li>
              <li>• Request a copy of your data</li>
            </ul>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">8. CONTACT US</h2>
            <p className="text-gray-600 leading-relaxed">
              For privacy-related inquiries, contact us at privacy@kayas.com or through our contact page.
            </p>
          </div>
        </div>
      </section>

      {/* Footer CTA */}
      <section className="bg-black text-white border-t border-black">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="flex justify-between items-center">
            <p className="text-gray-300">Questions about our privacy practices?</p>
            <Link href="/contact" className="px-8 h-12 border border-white flex items-center hover:bg-white/5 transition-colors font-medium tracking-wider">
              CONTACT US
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
