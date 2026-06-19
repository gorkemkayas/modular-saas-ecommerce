import type { Metadata } from "next"
import Link from "next/link"
import { Check, XCircle, ArrowRight, Store } from "lucide-react"

import { Button } from "@/components/ui/button"

export const dynamic = "force-dynamic"

export const metadata: Metadata = {
  title: "Payment Result | KAYAS",
  description: "Subscription payment result.",
}

interface PaymentResultPageProps {
  searchParams?: Promise<{
    status?: string
    planCode?: string
    error?: string
  }>
}

export default async function PaymentResultPage({
  searchParams,
}: PaymentResultPageProps) {
  const params = searchParams ? await searchParams : undefined
  const isSuccess = params?.status === "success"
  const planCode = params?.planCode ?? ""
  const error = params?.error ?? "Payment could not be completed."

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4 py-10">
      <div className="w-full max-w-md">
        <div className="mb-10">
          <Link
            href="/"
            className="font-serif text-xl font-light tracking-[0.22em] sm:text-2xl sm:tracking-[0.3em]"
          >
            KAYAS
          </Link>
        </div>

        {isSuccess ? (
          <>
            <div className="flex h-12 w-12 items-center justify-center rounded-md bg-primary text-primary-foreground">
              <Check className="h-5 w-5" strokeWidth={1.5} />
            </div>

            <div className="mt-8 mb-8">
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                Payment Successful
              </p>
              <h1 className="mt-3 font-serif text-3xl lg:text-4xl font-light tracking-wide">
                Your store is ready
              </h1>
              <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
                Your subscription has been activated with the{" "}
                <span className="font-medium text-foreground">{planCode}</span>{" "}
                plan. You can now sign in and start setting up your store.
              </p>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row">
              <Button asChild className="h-12 flex-1">
                <Link href="/auth/login">
                  Sign in to Admin
                  <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                </Link>
              </Button>
              <Button asChild variant="outline" className="h-12 flex-1">
                <Link href="/">
                  <Store className="h-4 w-4" strokeWidth={1.5} />
                  Home
                </Link>
              </Button>
            </div>
          </>
        ) : (
          <>
            <div className="flex h-12 w-12 items-center justify-center rounded-md bg-destructive text-destructive-foreground">
              <XCircle className="h-5 w-5" strokeWidth={1.5} />
            </div>

            <div className="mt-8 mb-8">
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                Payment Failed
              </p>
              <h1 className="mt-3 font-serif text-3xl lg:text-4xl font-light tracking-wide">
                Something went wrong
              </h1>
              <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
                {error}
              </p>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row">
              <Button asChild className="h-12 flex-1">
                <Link href="/store-register">
                  Try Again
                  <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                </Link>
              </Button>
              <Button asChild variant="outline" className="h-12 flex-1">
                <Link href="/">Home</Link>
              </Button>
            </div>
          </>
        )}
      </div>
    </main>
  )
}
