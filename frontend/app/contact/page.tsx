'use client';

import Link from 'next/link';
import { Mail, Phone, MapPin, Clock } from 'lucide-react';

export default function ContactPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="border-b border-black/10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <h1 className="text-5xl sm:text-6xl font-light tracking-wider mb-6">
            GET IN TOUCH
          </h1>
          <p className="text-lg text-gray-600 font-light">
            We&apos;re here to help. Reach out to us through any of our contact channels.
          </p>
        </div>
      </section>

      {/* Contact Information */}
      <section className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
          {/* Contact Methods */}
          <div>
            <h2 className="text-2xl font-light tracking-wider mb-12">CONTACT US</h2>
            
            <div className="space-y-8">
              {/* Email */}
              <div className="flex gap-4">
                <Mail className="w-6 h-6 flex-shrink-0 mt-1" />
                <div>
                  <h3 className="font-medium tracking-wide mb-2">Email</h3>
                  <p className="text-gray-600">support@kayas.com</p>
                  <p className="text-gray-600">info@kayas.com</p>
                </div>
              </div>

              {/* Phone */}
              <div className="flex gap-4">
                <Phone className="w-6 h-6 flex-shrink-0 mt-1" />
                <div>
                  <h3 className="font-medium tracking-wide mb-2">Phone</h3>
                  <p className="text-gray-600">+1 (555) 123-4567</p>
                  <p className="text-gray-600 text-sm">Mon-Fri: 9AM - 6PM EST</p>
                </div>
              </div>

              {/* Address */}
              <div className="flex gap-4">
                <MapPin className="w-6 h-6 flex-shrink-0 mt-1" />
                <div>
                  <h3 className="font-medium tracking-wide mb-2">Address</h3>
                  <p className="text-gray-600">KAYAS Headquarters</p>
                  <p className="text-gray-600">123 Fashion Avenue</p>
                  <p className="text-gray-600">New York, NY 10001</p>
                </div>
              </div>

              {/* Hours */}
              <div className="flex gap-4">
                <Clock className="w-6 h-6 flex-shrink-0 mt-1" />
                <div>
                  <h3 className="font-medium tracking-wide mb-2">Business Hours</h3>
                  <p className="text-gray-600">Monday - Friday: 9:00 AM - 6:00 PM EST</p>
                  <p className="text-gray-600">Saturday: 10:00 AM - 4:00 PM EST</p>
                  <p className="text-gray-600">Sunday: Closed</p>
                </div>
              </div>
            </div>
          </div>

          {/* Contact Form */}
          <div>
            <h2 className="text-2xl font-light tracking-wider mb-12">SEND US A MESSAGE</h2>
            
            <form className="space-y-6">
              <div>
                <label className="block text-sm font-medium mb-2">Full Name</label>
                <input
                  type="text"
                  placeholder="Your name"
                  className="w-full h-12 px-4 border border-black/20 bg-white text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Email Address</label>
                <input
                  type="email"
                  placeholder="your@email.com"
                  className="w-full h-12 px-4 border border-black/20 bg-white text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Subject</label>
                <input
                  type="text"
                  placeholder="How can we help?"
                  className="w-full h-12 px-4 border border-black/20 bg-white text-sm"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">Message</label>
                <textarea
                  placeholder="Your message here..."
                  rows={5}
                  className="w-full px-4 py-3 border border-black/20 bg-white text-sm resize-none"
                ></textarea>
              </div>

              <button className="w-full h-14 bg-black text-white font-medium tracking-wider hover:bg-black/90 transition-colors">
                SEND MESSAGE
              </button>
            </form>
          </div>
        </div>
      </section>

      {/* FAQ CTA */}
      <section className="bg-black text-white border-t border-black">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="text-2xl font-light tracking-wider mb-2">HAVE QUESTIONS?</h3>
              <p className="text-gray-300">Check our FAQ for quick answers</p>
            </div>
            <Link href="/help" className="px-8 h-12 border border-white flex items-center hover:bg-white/5 transition-colors font-medium tracking-wider">
              VISIT FAQ
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
