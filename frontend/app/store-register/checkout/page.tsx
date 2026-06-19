"use client"

import { Suspense, useEffect, useRef } from "react"
import { useSearchParams } from "next/navigation"
import Link from "next/link"

export default function CheckoutPage() {
  return (
    <Suspense>
      <CheckoutContent />
    </Suspense>
  )
}

function CheckoutContent() {
  const searchParams = useSearchParams()
  const containerRef = useRef<HTMLDivElement>(null)
  const formContent = searchParams.get("formContent")

  useEffect(() => {
    if (!formContent || !containerRef.current) return

    containerRef.current.innerHTML = formContent

    const scripts = containerRef.current.querySelectorAll("script")
    scripts.forEach((oldScript) => {
      const newScript = document.createElement("script")
      Array.from(oldScript.attributes).forEach((attr) =>
        newScript.setAttribute(attr.name, attr.value),
      )
      newScript.textContent = oldScript.textContent
      oldScript.parentNode?.replaceChild(newScript, oldScript)
    })
  }, [formContent])

  if (!formContent) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background px-4">
        <div className="text-center">
          <p className="text-sm text-muted-foreground">
            No checkout form available.
          </p>
          <Link
            href="/store-register"
            className="mt-4 inline-block text-sm underline underline-offset-4"
          >
            Back to registration
          </Link>
        </div>
      </main>
    )
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4 py-10">
      <div className="w-full max-w-lg">
        <div className="mb-8">
          <Link
            href="/"
            className="font-serif text-xl font-light tracking-[0.22em] sm:text-2xl sm:tracking-[0.3em]"
          >
            KAYAS
          </Link>
        </div>

        <div className="mb-6">
          <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
            Payment
          </p>
          <h1 className="mt-3 font-serif text-2xl font-light tracking-wide">
            Complete your subscription
          </h1>
        </div>

        <div ref={containerRef} id="iyzipay-checkout-form" />
      </div>
    </main>
  )
}
