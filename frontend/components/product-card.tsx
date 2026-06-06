"use client"

import Image from "next/image"
import Link from "next/link"
import { Eye, ShoppingBag } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useStore } from "@/lib/store-context"
import { formatMoney } from "@/lib/format"
import { toCartProductFromSummary } from "@/lib/storefront-adapters"
import type { StorefrontProductSummaryDto } from "@/lib/api/types"
import { storefrontPath } from "@/lib/config"
import { cn } from "@/lib/utils"

interface ProductCardProps {
  product: StorefrontProductSummaryDto
  storeSlug: string
  className?: string
}

export function ProductCard({
  product,
  storeSlug,
  className,
}: ProductCardProps) {
  const { addToCart } = useStore()
  const cartProduct = toCartProductFromSummary(product)
  const productHref = storefrontPath(storeSlug, `/products/${product.slug}`)

  const handleAddToCart = (event: React.MouseEvent) => {
    event.preventDefault()
    event.stopPropagation()

    if (!cartProduct) {
      return
    }

    addToCart(cartProduct)
  }

  return (
    <Link href={productHref} className={cn("group block", className)}>
      <div className="relative aspect-[3/4] overflow-hidden bg-secondary">
        <Image
          src={product.mainImageUrl || "/placeholder.jpg"}
          alt={product.name}
          fill
          className="object-cover transition-all duration-1000 ease-out group-hover:scale-105"
          sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 25vw"
        />

        {product.price?.isOnSale ? (
          <span className="absolute left-0 top-6 bg-foreground px-4 py-1.5 text-[9px] font-normal uppercase tracking-[0.2em] text-background">
            Sale
          </span>
        ) : null}

        <div className="absolute inset-0 flex items-center justify-center gap-3 bg-black/0 opacity-0 transition-all duration-500 group-hover:bg-black/20 group-hover:opacity-100">
          <Button
            size="icon"
            className="h-12 w-12 translate-y-4 bg-white text-black transition-all duration-300 hover:bg-white/90 group-hover:translate-y-0"
          >
            <Eye className="h-5 w-5" strokeWidth={1} />
            <span className="sr-only">View Product</span>
          </Button>

          {cartProduct ? (
            <Button
              onClick={handleAddToCart}
              size="icon"
              className="h-12 w-12 translate-y-4 bg-white text-black transition-all duration-500 hover:bg-white/90 group-hover:translate-y-0"
            >
              <ShoppingBag className="h-5 w-5" strokeWidth={1} />
              <span className="sr-only">Add to Cart</span>
            </Button>
          ) : null}
        </div>
      </div>

      <div className="mt-6">
        <p className="text-[9px] font-normal uppercase tracking-[0.3em] text-muted-foreground">
          {product.brandName || product.productType}
        </p>
        <h3 className="mt-2 font-serif text-lg font-light tracking-wide text-foreground transition-colors group-hover:text-foreground/70">
          {product.name}
        </h3>

        <div className="mt-3 flex items-center gap-4">
          {product.price ? (
            <>
              <span className="text-sm tracking-wide text-foreground">
                {formatMoney(product.price.amount, product.price.currencyCode)}
              </span>
              {product.price.compareAtAmount ? (
                <span className="text-sm tracking-wide text-muted-foreground line-through">
                  {formatMoney(
                    product.price.compareAtAmount,
                    product.price.currencyCode,
                  )}
                </span>
              ) : null}
            </>
          ) : (
            <span className="text-sm tracking-wide text-muted-foreground">
              Price unavailable
            </span>
          )}
        </div>
      </div>
    </Link>
  )
}
