"use client"

import Link from "next/link"
import { use } from "react"
import { useSearchParams } from "next/navigation"
import { CheckCircle, Clock, RefreshCw, XCircle } from "lucide-react"
import { Button } from "@/components/ui/button"
import { getAccountPath } from "@/lib/account-path"

type PaymentStatus = "success" | "failed" | "pending" | "processing"

const statusConfig = {
  success: {
    icon: CheckCircle,
    title: "Payment Successful",
    description: "Your payment has been processed successfully.",
  },
  failed: {
    icon: XCircle,
    title: "Payment Failed",
    description: "We couldn't process your payment. Please try again.",
  },
  pending: {
    icon: Clock,
    title: "Payment Pending",
    description: "Your payment is still being reviewed.",
  },
  processing: {
    icon: RefreshCw,
    title: "Processing Payment",
    description: "Please wait while we confirm the provider response.",
  },
} as const

export function StorePaymentResultContent({
  params,
}: {
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = use(params)
  const ordersHref = getAccountPath(storeSlug, "/orders")
  const searchParams = useSearchParams()
  const status = (searchParams.get("status") as PaymentStatus) || "processing"
  const orderId = searchParams.get("orderId")
  const config = statusConfig[status] ?? statusConfig.processing
  const Icon = config.icon

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-6">
      <div className="max-w-md text-center">
        <div className="mx-auto mb-8 flex h-20 w-20 items-center justify-center bg-foreground">
          <Icon
            className={`h-10 w-10 text-background ${status === "processing" ? "animate-spin" : ""}`}
            strokeWidth={1}
          />
        </div>

        <h1 className="mb-4 font-serif text-3xl font-light tracking-wide lg:text-4xl">
          {config.title}
        </h1>
        <p className="mb-8 text-muted-foreground">{config.description}</p>

        {orderId ? (
          <p className="mb-8 text-sm text-muted-foreground">Order reference: {orderId}</p>
        ) : null}

        <div className="space-y-4">
          {status === "success" && orderId ? (
            <Link href={`/${storeSlug}/order-success?orderId=${orderId}`}>
              <Button className="h-12 px-8 bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase">
                View Order Confirmation
              </Button>
            </Link>
          ) : null}

          {status === "failed" ? (
            <Link href={`/${storeSlug}/checkout`}>
              <Button className="h-12 px-8 bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase">
                Retry Checkout
              </Button>
            </Link>
          ) : null}

          <div>
            <Link
              href={ordersHref}
              className="text-sm text-muted-foreground underline underline-offset-4 transition-colors hover:text-foreground"
            >
              Open My Orders
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
