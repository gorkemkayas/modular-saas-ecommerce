'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Search, Filter, ChevronRight } from 'lucide-react';

export default function AdminOrdersPage() {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  const orders = [
    {
      id: 'ORD-2024-001',
      customer: 'John Doe',
      date: '2024-05-01',
      total: '$299.00',
      paymentStatus: 'Paid',
      fulfillmentStatus: 'Delivered',
    },
    {
      id: 'ORD-2024-002',
      customer: 'Jane Smith',
      date: '2024-05-02',
      total: '$449.00',
      paymentStatus: 'Paid',
      fulfillmentStatus: 'Shipping',
    },
    {
      id: 'ORD-2024-003',
      customer: 'Alice Johnson',
      date: '2024-05-03',
      total: '$199.00',
      paymentStatus: 'Pending',
      fulfillmentStatus: 'Processing',
    },
  ];

  return (
    <div className="max-w-7xl">
      <div className="mb-8">
        <h1 className="text-2xl font-light tracking-tight mb-6">Orders</h1>

        <div className="flex flex-col md:flex-row gap-4 mb-6">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" strokeWidth={1} />
            <input
              type="text"
              placeholder="Search by order ID or customer..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-3 border border-gray-300 focus:outline-none focus:border-black text-sm"
            />
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="px-4 py-3 border border-gray-300 focus:outline-none focus:border-black text-sm"
          >
            <option value="all">All Status</option>
            <option value="processing">Processing</option>
            <option value="shipping">Shipping</option>
            <option value="delivered">Delivered</option>
          </select>
        </div>
      </div>

      <div className="border border-gray-200 overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="border-b border-gray-200 bg-gray-50">
            <tr>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Order ID</th>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Customer</th>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Date</th>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Total</th>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Payment</th>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Fulfillment</th>
              <th className="px-6 py-4 text-left font-light tracking-widest uppercase">Action</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id} className="border-b border-gray-200 hover:bg-gray-50 transition-colors">
                <td className="px-6 py-4 font-light">{order.id}</td>
                <td className="px-6 py-4 font-light">{order.customer}</td>
                <td className="px-6 py-4 font-light">{order.date}</td>
                <td className="px-6 py-4 font-light">{order.total}</td>
                <td className="px-6 py-4">
                  <span className="px-3 py-1 bg-green-100 text-green-800 text-xs font-light tracking-widest uppercase">
                    {order.paymentStatus}
                  </span>
                </td>
                <td className="px-6 py-4">
                  <span className="px-3 py-1 bg-blue-100 text-blue-800 text-xs font-light tracking-widest uppercase">
                    {order.fulfillmentStatus}
                  </span>
                </td>
                <td className="px-6 py-4">
                  <Link
                    href={`/admin/orders/${order.id}`}
                    className="text-black hover:text-gray-600 transition-colors"
                  >
                    <ChevronRight className="w-4 h-4" strokeWidth={1} />
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
