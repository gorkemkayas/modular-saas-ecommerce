"use client"

import Image from "next/image"
import Link from "next/link"
import { ArrowRight, Minus, Plus, ShoppingBag, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { storefrontPath } from "@/lib/config"
import { formatMoney } from "@/lib/format"
import { useStore } from "@/lib/store-context"

interface CartContentProps {
  storeSlug: string
}

export function CartContent({ storeSlug }: CartContentProps) {
  const { cart, removeFromCart, updateQuantity, getCartTotal } = useStore()

  if (!cart.length) {
    return (
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center py-32 text-center">
          <div className="flex h-24 w-24 items-center justify-center border border-border">
            <ShoppingBag className="h-10 w-10 text-muted-foreground" strokeWidth={1} />
          </div>
          <h1 className="mt-10 font-serif text-4xl font-light text-foreground">
            Cart is Empty
          </h1>
          <p className="mt-4 text-sm text-muted-foreground">
            Add published storefront products to continue with checkout.
          </p>
          <Button asChild className="mt-10 h-14 px-12 text-[11px] uppercase tracking-[0.2em]">
            <Link href={storefrontPath(storeSlug, "/products")}>
              Browse Catalog
              <ArrowRight className="ml-3 h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>
    )
  }

  const subtotal = getCartTotal()
  const currencyCode = cart[0]?.currencyCode ?? "TRY"
  const shipping = subtotal > 0 ? 0 : 0
  const total = subtotal + shipping

  return (
    <div className="mx-auto max-w-7xl px-6 lg:px-8">
      <div className="flex items-baseline justify-between">
        <h1 className="font-serif text-4xl font-light tracking-tight text-foreground lg:text-5xl">
          Cart
        </h1>
        <p className="text-[11px] uppercase tracking-[0.25em] text-muted-foreground">
          {cart.length} lines
        </p>
      </div>

      <div className="mt-16 grid gap-16 lg:grid-cols-3 lg:gap-24">
        <div className="lg:col-span-2">
          <div className="divide-y divide-border">
            {cart.map((item) => (
              <div
                key={`${item.productId}-${item.variantId ?? "base"}`}
                className="flex gap-6 py-10"
              >
                <Link
                  href={storefrontPath(storeSlug, `/products/${item.productSlug}`)}
                  className="shrink-0"
                >
                  <div className="relative h-32 w-24 overflow-hidden bg-secondary sm:h-40 sm:w-32">
                    <Image
                      src={item.imageUrl || "/placeholder.jpg"}
                      alt={item.name}
                      fill
                      className="object-cover transition-transform duration-500 hover:scale-105"
                    />
                  </div>
                </Link>

                <div className="flex flex-1 flex-col justify-between">
                  <div>
                    <div className="flex items-start justify-between">
                      <div>
                        <p className="text-[9px] font-normal uppercase tracking-[0.3em] text-muted-foreground">
                          {item.categoryName || item.brandName || "Storefront"}
                        </p>
                        <Link
                          href={storefrontPath(storeSlug, `/products/${item.productSlug}`)}
                          className="mt-1 block font-serif text-lg font-light text-foreground transition-colors hover:text-muted-foreground"
                        >
                          {item.name}
                        </Link>

                        {item.variantName || item.selectedOptions.length ? (
                          <div className="mt-2 space-y-1 text-xs text-muted-foreground">
                            {item.variantName ? <p>{item.variantName}</p> : null}
                            {item.selectedOptions.map((option) => (
                              <p key={`${item.productId}-${option}`}>{option}</p>
                            ))}
                          </div>
                        ) : null}
                      </div>

                      <p className="text-sm tracking-wide text-foreground">
                        {formatMoney(item.priceAmount * item.quantity, item.currencyCode)}
                      </p>
                    </div>
                  </div>

                  <div className="mt-6 flex items-center justify-between">
                    <div className="flex items-center border border-border">
                      <button
                        onClick={() =>
                          updateQuantity(item.productId, item.quantity - 1, item.variantId)
                        }
                        className="flex h-10 w-10 items-center justify-center text-muted-foreground transition-colors hover:text-foreground"
                      >
                        <Minus className="h-3 w-3" strokeWidth={1} />
                      </button>
                      <span className="flex h-10 w-12 items-center justify-center text-xs font-light text-foreground">
                        {item.quantity}
                      </span>
                      <button
                        onClick={() =>
                          updateQuantity(item.productId, item.quantity + 1, item.variantId)
                        }
                        className="flex h-10 w-10 items-center justify-center text-muted-foreground transition-colors hover:text-foreground"
                      >
                        <Plus className="h-3 w-3" strokeWidth={1} />
                      </button>
                    </div>
                    <button
                      onClick={() => removeFromCart(item.productId, item.variantId)}
                      className="premium-link flex items-center gap-2 text-[10px] uppercase tracking-[0.2em] text-muted-foreground transition-colors hover:text-foreground"
                    >
                      <Trash2 className="h-4 w-4" strokeWidth={1} />
                      Remove
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="lg:col-span-1">
          <div className="sticky top-32 border border-border bg-card p-10">
            <h2 className="text-[11px] font-normal uppercase tracking-[0.25em] text-foreground">
              Order Summary
            </h2>

            <div className="mt-10 space-y-5">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="tracking-wide text-foreground">
                  {formatMoney(subtotal, currencyCode)}
                </span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Shipping</span>
                <span className="tracking-wide text-foreground">Calculated later</span>
              </div>
              <div className="h-px w-full bg-border" />
              <div className="flex items-center justify-between pt-2">
                <span className="text-sm text-foreground">Total</span>
                <span className="text-xl tracking-wide text-foreground">
                  {formatMoney(total, currencyCode)}
                </span>
              </div>
            </div>

            <Button asChild className="mt-10 h-14 w-full text-[11px] uppercase tracking-[0.2em]">
              <Link href={storefrontPath(storeSlug, "/checkout")}>
                Proceed to Checkout
                <ArrowRight className="ml-3 h-4 w-4" />
              </Link>
            </Button>

            <Link
              href={storefrontPath(storeSlug, "/products")}
              className="premium-link mt-6 block text-center text-[10px] uppercase tracking-[0.2em] text-muted-foreground transition-colors hover:text-foreground"
            >
              Continue Shopping
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
