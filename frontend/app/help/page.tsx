'use client';

import { ChevronDown } from 'lucide-react';
import { useState } from 'react';
import Link from 'next/link';

const faqs = [
  {
    category: 'Orders & Shipping',
    items: [
      {
        q: 'How long does shipping take?',
        a: 'Standard shipping takes 5-7 business days, express shipping 2-3 business days, and overnight shipping 1 business day. Shipping times begin after order processing.',
      },
      {
        q: 'How can I track my order?',
        a: 'Once shipped, you&apos;ll receive a tracking number via email. Use this number to track your shipment on the carrier&apos;s website or in your KAYAS account.',
      },
      {
        q: 'Do you ship internationally?',
        a: 'Yes, we ship to Canada and select international destinations. International shipping rates and times vary. Customers are responsible for customs duties.',
      },
      {
        q: 'What if my package arrives damaged?',
        a: 'Contact us immediately at support@kayas.com with photos and tracking information. We&apos;ll work with the carrier to replace or refund your order.',
      },
    ],
  },
  {
    category: 'Returns & Exchanges',
    items: [
      {
        q: 'What is your return policy?',
        a: 'You have 30 days from purchase to return items in original condition with tags attached. Final sale items cannot be returned.',
      },
      {
        q: 'How do I return an item?',
        a: 'Log into your account, select the order, and click "Return". We&apos;ll provide a prepaid return label. Ship the item back, and we&apos;ll process your refund within 5-7 business days.',
      },
      {
        q: 'Can I exchange for a different size or color?',
        a: 'Yes, exchanges are free. When initiating your return, select exchange instead of refund and specify the size or color you&apos;d like.',
      },
      {
        q: 'How long does refund processing take?',
        a: 'Once we receive your return, it takes 5-7 business days to process. Allow 3-5 additional business days for the refund to appear in your account.',
      },
    ],
  },
  {
    category: 'Account & Security',
    items: [
      {
        q: 'How do I create an account?',
        a: 'Click "Sign Up" or "Register" and provide your email, password, and basic information. You can also sign up with Google.',
      },
      {
        q: 'How do I reset my password?',
        a: 'Click "Forgot Password" on the login page and follow the instructions. We&apos;ll send a password reset link to your email.',
      },
      {
        q: 'Is my information secure?',
        a: 'Yes, we use encryption and industry-standard security measures. Payment information is processed through secure payment gateways.',
      },
      {
        q: 'Can I delete my account?',
        a: 'Yes, you can delete your account from your account settings. This action cannot be undone. Contact support@kayas.com if you need assistance.',
      },
    ],
  },
  {
    category: 'Payments & Pricing',
    items: [
      {
        q: 'What payment methods do you accept?',
        a: 'We accept all major credit cards (Visa, Mastercard, American Express), PayPal, and Apple Pay.',
      },
      {
        q: 'Is my payment information stored?',
        a: 'We don&apos;t store full payment details. You can choose to save a payment method for future purchases with your consent.',
      },
      {
        q: 'Why did my price change?',
        a: 'Prices may vary due to sales, promotions, or discounts applied during checkout. Your cart always shows the most current price.',
      },
      {
        q: 'Do you offer gift cards?',
        a: 'Yes, we offer digital and physical gift cards. Purchase them on our website and give the code to the recipient.',
      },
    ],
  },
  {
    category: 'Products & Quality',
    items: [
      {
        q: 'Are your products made ethically?',
        a: 'Yes, we&apos;re committed to ethical manufacturing and sustainable practices. All production facilities meet strict labor and environmental standards.',
      },
      {
        q: 'What is your size guide?',
        a: 'Visit our size guide page for detailed measurements. Each product has size recommendations and fit notes in the description.',
      },
      {
        q: 'Do you offer custom sizing?',
        a: 'For bulk orders or special requests, contact our customer service team at support@kayas.com.',
      },
      {
        q: 'How do I care for my KAYAS pieces?',
        a: 'Each item includes care instructions on the tag and in your order confirmation. We recommend gentle washing and proper storage.',
      },
    ],
  },
  {
    category: 'Promotions & Discounts',
    items: [
      {
        q: 'How do I get a discount code?',
        a: 'Sign up for our newsletter to receive exclusive offers. Follow us on social media for flash sales and special promotions.',
      },
      {
        q: 'Can I combine coupons?',
        a: 'Typically only one discount code can be applied per order. Check the terms of your coupon for any restrictions.',
      },
      {
        q: 'Do you offer student or corporate discounts?',
        a: 'We offer select discounts to verified students and some corporate partners. Contact support@kayas.com for information.',
      },
      {
        q: 'When do you have sales?',
        a: 'We host seasonal sales, flash sales, and special promotions throughout the year. Subscribe to our newsletter to stay informed.',
      },
    ],
  },
];

function FAQItem({ item }: { item: { q: string; a: string } }) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="border-b border-black/10 py-4">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-start justify-between gap-4 hover:text-black/70 transition-colors"
      >
        <h3 className="text-left font-medium tracking-wide">{item.q}</h3>
        <ChevronDown
          className={`w-5 h-5 flex-shrink-0 mt-1 transition-transform ${
            isOpen ? 'rotate-180' : ''
          }`}
        />
      </button>
      {isOpen && <p className="text-gray-600 mt-4 leading-relaxed">{item.a}</p>}
    </div>
  );
}

export default function HelpPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="border-b border-black/10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <h1 className="text-5xl sm:text-6xl font-light tracking-wider mb-6">
            HELP & FAQ
          </h1>
          <p className="text-lg text-gray-600 font-light">
            Find answers to common questions about KAYAS
          </p>
        </div>
      </section>

      {/* Quick Links */}
      <section className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-16 border-b border-black/10">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          <Link href="/contact" className="group">
            <div className="border border-black/20 p-8 hover:border-black transition-colors">
              <h3 className="text-lg font-medium tracking-wider mb-2">Contact Us</h3>
              <p className="text-gray-600 text-sm mb-4">Reach out directly to our customer service team</p>
              <span className="text-sm font-medium group-hover:translate-x-1 transition-transform inline-block">
                Contact →
              </span>
            </div>
          </Link>

          <Link href="/shipping-policy" className="group">
            <div className="border border-black/20 p-8 hover:border-black transition-colors">
              <h3 className="text-lg font-medium tracking-wider mb-2">Shipping</h3>
              <p className="text-gray-600 text-sm mb-4">Learn about our shipping methods and timeline</p>
              <span className="text-sm font-medium group-hover:translate-x-1 transition-transform inline-block">
                Learn more →
              </span>
            </div>
          </Link>

          <Link href="/return-policy" className="group">
            <div className="border border-black/20 p-8 hover:border-black transition-colors">
              <h3 className="text-lg font-medium tracking-wider mb-2">Returns</h3>
              <p className="text-gray-600 text-sm mb-4">Our returns and exchange policy details</p>
              <span className="text-sm font-medium group-hover:translate-x-1 transition-transform inline-block">
                Learn more →
              </span>
            </div>
          </Link>
        </div>
      </section>

      {/* FAQ Sections */}
      <section className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        {faqs.map((section) => (
          <div key={section.category} className="mb-16">
            <h2 className="text-2xl font-light tracking-wider mb-8">{section.category.toUpperCase()}</h2>
            <div className="space-y-0">
              {section.items.map((item, idx) => (
                <FAQItem key={idx} item={item} />
              ))}
            </div>
          </div>
        ))}
      </section>

      {/* Contact CTA */}
      <section className="bg-black text-white border-t border-black">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <div className="text-center">
            <h2 className="text-3xl font-light tracking-wider mb-4">DIDN&apos;T FIND YOUR ANSWER?</h2>
            <p className="text-gray-300 mb-8">Our customer service team is here to help</p>
            <Link href="/contact" className="inline-flex items-center justify-center h-14 px-8 border border-white hover:bg-white/5 transition-colors font-medium tracking-wider">
              CONTACT SUPPORT
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
