'use client';

import Link from 'next/link';

export default function TermsPage() {
  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="border-b border-black/10">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <h1 className="text-4xl sm:text-5xl font-light tracking-wider mb-4">
            TERMS OF SERVICE
          </h1>
          <p className="text-gray-600 font-light">Last updated: May 2024</p>
        </div>
      </section>

      {/* Content */}
      <section className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="space-y-12">
          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">1. ACCEPTANCE OF TERMS</h2>
            <p className="text-gray-600 leading-relaxed">
              By accessing and using this website and services, you accept and agree to be bound by the terms and provision of this agreement. If you do not agree to abide by the above, please do not use this service.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">2. USE LICENSE</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Permission is granted to temporarily download one copy of the materials (information or software) on KAYAS website for personal, non-commercial transitory viewing only. This is the grant of a license, not a transfer of title, and under this license you may not:
            </p>
            <ul className="space-y-3 text-gray-600">
              <li>• Modifying or copying the materials</li>
              <li>• Using the materials for any commercial purpose or for any public display</li>
              <li>• Attempting to decompile or reverse engineer any software</li>
              <li>• Removing any copyright or other proprietary notations</li>
              <li>• Transferring the materials to another person or &quot;mirroring&quot; the materials on any other server</li>
            </ul>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">3. DISCLAIMER</h2>
            <p className="text-gray-600 leading-relaxed">
              The materials on KAYAS website are provided on an &apos;as is&apos; basis. KAYAS makes no warranties, expressed or implied, and hereby disclaims and negates all other warranties including, without limitation, implied warranties or conditions of merchantability, fitness for a particular purpose, or non-infringement of intellectual property or other violation of rights.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">4. LIMITATIONS</h2>
            <p className="text-gray-600 leading-relaxed">
              In no event shall KAYAS or its suppliers be liable for any damages (including, without limitation, damages for loss of data or profit, or due to business interruption) arising out of the use or inability to use the materials on KAYAS website.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">5. ACCURACY OF MATERIALS</h2>
            <p className="text-gray-600 leading-relaxed">
              The materials appearing on KAYAS website could include technical, typographical, or photographic errors. KAYAS does not warrant that any of the materials on its website are accurate, complete, or current. KAYAS may make changes to the materials contained on its website at any time without notice.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">6. LINKS</h2>
            <p className="text-gray-600 leading-relaxed">
              KAYAS has not reviewed all of the sites linked to its website and is not responsible for the contents of any such linked site. The inclusion of any link does not imply endorsement by KAYAS of the site. Use of any such linked website is at the user&apos;s own risk.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">7. MODIFICATIONS</h2>
            <p className="text-gray-600 leading-relaxed">
              KAYAS may revise these terms of service for its website at any time without notice. By using this website, you are agreeing to be bound by the then current version of these terms of service.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">8. GOVERNING LAW</h2>
            <p className="text-gray-600 leading-relaxed">
              These terms and conditions are governed by and construed in accordance with the laws of the United States, and you irrevocably submit to the exclusive jurisdiction of the courts in New York, New York.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">9. RETURNS & REFUNDS</h2>
            <p className="text-gray-600 leading-relaxed mb-4">
              Please refer to our Return Policy for information regarding returns and refunds. All sale items are final and cannot be returned or exchanged.
            </p>
          </div>

          <div>
            <h2 className="text-2xl font-light tracking-wider mb-4">10. CONTACT US</h2>
            <p className="text-gray-600 leading-relaxed">
              If you have any questions about these Terms, please contact us at legal@kayas.com.
            </p>
          </div>
        </div>
      </section>

      {/* Footer CTA */}
      <section className="bg-black text-white border-t border-black">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="flex justify-between items-center">
            <p className="text-gray-300">Agree to our terms?</p>
            <Link href="/products" className="px-8 h-12 border border-white flex items-center hover:bg-white/5 transition-colors font-medium tracking-wider">
              START SHOPPING
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
