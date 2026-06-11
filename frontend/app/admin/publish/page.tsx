'use client';

import Link from 'next/link';
import { ChevronRight, Eye, EyeOff } from 'lucide-react';

export default function AdminPublishPage() {
  return (
    <div className="max-w-4xl">
      <nav className="flex items-center gap-2 text-sm mb-8 uppercase tracking-widest">
        <Link href="/admin/store-settings" className="text-gray-500 hover:text-black">
          Settings
        </Link>
        <ChevronRight className="w-4 h-4 text-gray-400" />
        <span className="text-black">Publish</span>
      </nav>

      <h1 className="text-3xl font-light tracking-tight mb-8">Store Publish Status</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
        <div className="border border-gray-200 p-8">
          <h2 className="text-lg font-light tracking-tight mb-4">Current Status</h2>
          <div className="space-y-6">
            <div>
              <p className="text-xs text-gray-600 font-light tracking-widest uppercase mb-2">Store Visibility</p>
              <div className="flex items-center gap-2 mb-4">
                <Eye className="w-5 h-5" strokeWidth={1} />
                <p className="font-light text-lg">Published</p>
              </div>
              <p className="text-sm text-green-600 font-light">Your store is live and visible to customers</p>
            </div>

            <div>
              <p className="text-xs text-gray-600 font-light tracking-widest uppercase mb-2">Store URL</p>
              <p className="font-light">https://kayas.shop</p>
            </div>

            <div>
              <p className="text-xs text-gray-600 font-light tracking-widest uppercase mb-2">Last Published</p>
              <p className="font-light">May 3, 2024 at 2:30 PM</p>
            </div>
          </div>
        </div>

        <div className="border border-gray-200 p-8">
          <h2 className="text-lg font-light tracking-tight mb-6">Actions</h2>
          <div className="space-y-3">
            <button className="w-full px-6 py-3 border border-gray-300 font-light tracking-widest uppercase text-sm hover:bg-gray-50 transition-colors">
              Preview Store
            </button>
            <button className="w-full px-6 py-3 border border-gray-300 font-light tracking-widest uppercase text-sm hover:bg-gray-50 transition-colors">
              View Published
            </button>
            <button className="w-full px-6 py-3 bg-black text-white font-light tracking-widest uppercase text-sm hover:bg-gray-800 transition-colors">
              Publish Changes
            </button>
            <button className="w-full px-6 py-3 border border-red-300 text-red-600 font-light tracking-widest uppercase text-sm hover:bg-red-50 transition-colors">
              Unpublish Store
            </button>
          </div>
        </div>
      </div>

      <div className="border border-gray-200 p-8 mb-8">
        <h2 className="text-lg font-light tracking-tight mb-6">Pre-Launch Checklist</h2>
        <div className="space-y-4">
          {[
            { item: 'Store name and logo configured', done: true },
            { item: 'At least 3 products added', done: true },
            { item: 'Payment methods configured', done: true },
            { item: 'Shipping rates set', done: true },
            { item: 'Policies (Privacy, Terms) added', done: true },
            { item: 'Contact information complete', done: false },
            { item: 'Custom domain configured', done: false },
          ].map((check, idx) => (
            <label key={idx} className="flex items-center gap-3 cursor-pointer">
              <input type="checkbox" defaultChecked={check.done} className="w-4 h-4" disabled />
              <span className={`font-light text-sm ${check.done ? '' : 'text-gray-500'}`}>{check.item}</span>
            </label>
          ))}
        </div>
      </div>

      <div className="border border-yellow-200 bg-yellow-50 p-6">
        <p className="text-sm font-light text-yellow-900">
          Complete all checklist items before publishing your store to ensure a smooth launch.
        </p>
      </div>
    </div>
  );
}
