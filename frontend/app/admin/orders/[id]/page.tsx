'use client';

import Link from 'next/link';
import { ChevronRight, Calendar, CreditCard, Package, MapPin } from 'lucide-react';

export default function AdminOrderDetailPage({ params }: { params: { id: string } }) {
  return (
    <div className="max-w-7xl">
      <nav className="flex items-center gap-2 text-sm mb-8 uppercase tracking-widest">
        <Link href="/admin/orders" className="text-gray-500 hover:text-black">
          Orders
        </Link>
        <ChevronRight className="w-4 h-4 text-gray-400" />
        <span className="text-black">{params.id}</span>
      </nav>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
        <div className="md:col-span-2">
          <h1 className="text-3xl font-light tracking-tight mb-8">Order {params.id}</h1>

          <div className="border border-gray-200 p-8 mb-8">
            <h2 className="text-lg font-light tracking-tight mb-6">Order Summary</h2>
            <div className="grid grid-cols-2 gap-6">
              <div>
                <p className="text-xs text-gray-600 mb-2 font-light tracking-widest uppercase">Customer</p>
                <p className="font-light">John Doe</p>
                <p className="text-sm text-gray-600 font-light">john@example.com</p>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-2 font-light tracking-widest uppercase">Order Date</p>
                <p className="font-light">May 1, 2024 at 2:30 PM</p>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-2 font-light tracking-widest uppercase">Total Amount</p>
                <p className="text-2xl font-light">$299.00</p>
              </div>
              <div>
                <p className="text-xs text-gray-600 mb-2 font-light tracking-widest uppercase">Payment Status</p>
                <span className="px-3 py-1 bg-green-100 text-green-800 text-xs font-light tracking-widest uppercase">
                  Paid
                </span>
              </div>
            </div>
          </div>

          <div className="border border-gray-200 p-8 mb-8">
            <h2 className="text-lg font-light tracking-tight mb-6">Items</h2>
            <div className="space-y-4">
              {[1, 2].map((item) => (
                <div key={item} className="flex justify-between items-start border-b border-gray-200 pb-4">
                  <div>
                    <p className="font-light">Premium Cotton T-Shirt</p>
                    <p className="text-sm text-gray-600 font-light">Black • Size M</p>
                  </div>
                  <div className="text-right">
                    <p className="font-light">Qty: 1</p>
                    <p className="text-sm text-gray-600 font-light">$149.50</p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="border border-gray-200 p-8">
            <h2 className="text-lg font-light tracking-tight mb-6">Shipping Address</h2>
            <p className="font-light mb-1">John Doe</p>
            <p className="text-sm text-gray-600 font-light">123 Main Street</p>
            <p className="text-sm text-gray-600 font-light">Istanbul, 34000 Turkey</p>
          </div>
        </div>

        <div>
          <div className="border border-gray-200 p-6 sticky top-32">
            <h2 className="text-lg font-light tracking-tight mb-6">Actions</h2>
            <div className="space-y-3">
              <button className="w-full px-4 py-3 bg-black text-white font-light tracking-widest uppercase text-sm hover:bg-gray-800 transition-colors">
                Edit Order
              </button>
              <button className="w-full px-4 py-3 border border-gray-300 font-light tracking-widest uppercase text-sm hover:bg-gray-50 transition-colors">
                Send Invoice
              </button>
              <button className="w-full px-4 py-3 border border-gray-300 font-light tracking-widest uppercase text-sm hover:bg-gray-50 transition-colors">
                Create Shipment
              </button>
              <button className="w-full px-4 py-3 border border-red-300 text-red-600 font-light tracking-widest uppercase text-sm hover:bg-red-50 transition-colors">
                Cancel Order
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
