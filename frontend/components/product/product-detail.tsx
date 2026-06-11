"use client"

import { useEffect, useMemo, useState, type MouseEvent, type TouchEvent } from "react"
import Image from "next/image"
import Link from "next/link"
import {
  ChevronLeft,
  Heart,
  Minus,
  Plus,
  RotateCcw,
  Shield,
  ShoppingBag,
  Truck,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import type { StorefrontProductDto } from "@/lib/api/types"
import { storefrontPath } from "@/lib/config"
import { formatMoney } from "@/lib/format"
import { useStore } from "@/lib/store-context"
import { toCartProductFromDetail } from "@/lib/storefront-adapters"
import { cn } from "@/lib/utils"

interface ProductDetailProps {
  product: StorefrontProductDto
  storeSlug: string
}

type StorefrontVariant = StorefrontProductDto["variants"][number]
type VariantAttributeDefinition = {
  attributeDefinitionId: string
  name: string
  code: string
}

function normalizeAttributeCode(value: string | null | undefined): string {
  return (value ?? "").trim().toLowerCase()
}

function getVariantAttributeValue(
  variant: StorefrontVariant,
  attributeDefinitionId: string,
): string | null {
  return (
    variant.attributes.find(
      (attribute) => attribute.attributeDefinitionId === attributeDefinitionId,
    )?.value ?? null
  )
}

function isHexColor(value: string): boolean {
  return /^#(?:[0-9a-fA-F]{3}){1,2}$/.test(value.trim())
}

function resolveColorValue(
  attributes: StorefrontProductDto["variants"][number]["attributes"],
): string | null {
  const colorAttribute = attributes.find((attribute) => {
    const normalizedCode = normalizeAttributeCode(attribute.code)
    return normalizedCode === "color" || normalizedCode === "colour"
  })

  return colorAttribute?.value.trim().toLowerCase() || null
}

function resolveVariantOptionLabel(
  attribute: VariantAttributeDefinition,
  optionValue: string,
  variants: StorefrontVariant[],
): string {
  const normalizedCode = normalizeAttributeCode(attribute.code)

  if ((normalizedCode === "color" || normalizedCode === "colour") && isHexColor(optionValue)) {
    const namedVariant = variants.find(
      (variant) =>
        getVariantAttributeValue(variant, attribute.attributeDefinitionId) === optionValue &&
        variant.name?.trim(),
    )

    if (namedVariant?.name?.trim()) {
      return namedVariant.name.trim()
    }
  }

  return optionValue
}

export function ProductDetail({ product, storeSlug }: ProductDetailProps) {
  const { addToCart } = useStore()
  const [quantity, setQuantity] = useState(1)
  const [selectedVariantId, setSelectedVariantId] = useState<string | null>(
    product.variants[0]?.id ?? null,
  )
  const [selectedMediaIndex, setSelectedMediaIndex] = useState(0)
  const [isImageZoomed, setIsImageZoomed] = useState(false)
  const [imageZoomOrigin, setImageZoomOrigin] = useState({ x: 50, y: 50 })

  const selectedVariant = useMemo(
    () =>
      selectedVariantId
        ? product.variants.find((item) => item.id === selectedVariantId) ?? null
        : null,
    [product.variants, selectedVariantId],
  )

  const variantAttributeDefinitions = useMemo(() => {
    const seen = new Set<string>()
    const definitions: VariantAttributeDefinition[] = []

    for (const variant of product.variants) {
      for (const attribute of variant.attributes) {
        if (!attribute.isVariantDefining || seen.has(attribute.attributeDefinitionId)) {
          continue
        }

        seen.add(attribute.attributeDefinitionId)
        definitions.push({
          attributeDefinitionId: attribute.attributeDefinitionId,
          name: attribute.name,
          code: attribute.code,
        })
      }
    }

    return definitions
  }, [product.variants])

  const selectedAttributeValues = useMemo(() => {
    return new Map(
      (selectedVariant?.attributes ?? [])
        .filter((attribute) => attribute.isVariantDefining)
        .map((attribute) => [attribute.attributeDefinitionId, attribute.value] as const),
    )
  }, [selectedVariant])

  const resolvedPrice = selectedVariant?.price ?? product.price
  const saleDiscountPercentage = useMemo(() => {
    if (!resolvedPrice?.compareAtAmount || resolvedPrice.compareAtAmount <= resolvedPrice.amount) {
      return null
    }

    return Math.round(
      ((resolvedPrice.compareAtAmount - resolvedPrice.amount) / resolvedPrice.compareAtAmount) *
        100,
    )
  }, [resolvedPrice])

  const mediaItems = useMemo(() => {
    if (!selectedVariant) {
      return product.mediaItems
    }

    if (selectedVariant.mediaItems.length) {
      return selectedVariant.mediaItems
    }

    const selectedVariantColor = resolveColorValue(selectedVariant.attributes)
    if (selectedVariantColor) {
      const sameColorVariantWithMedia = product.variants.find((variant) => {
        if (!variant.mediaItems.length || variant.id === selectedVariant.id) {
          return false
        }

        return resolveColorValue(variant.attributes) === selectedVariantColor
      })

      if (sameColorVariantWithMedia) {
        return sameColorVariantWithMedia.mediaItems
      }
    }

    return product.mediaItems
  }, [product.mediaItems, product.variants, selectedVariant])

  const defaultMediaIndex = useMemo(() => {
    const mainIndex = mediaItems.findIndex((item) => item.isMain)
    return mainIndex >= 0 ? mainIndex : 0
  }, [mediaItems])

  const resolvedMediaIndex =
    selectedMediaIndex >= 0 && selectedMediaIndex < mediaItems.length
      ? selectedMediaIndex
      : defaultMediaIndex

  const mainMediaItem = mediaItems[resolvedMediaIndex] ?? null

  const cartProduct = toCartProductFromDetail(
    product,
    selectedVariantId,
    mainMediaItem?.url ?? null,
  )

  useEffect(() => {
    setSelectedMediaIndex((currentIndex) => {
      if (currentIndex >= 0 && currentIndex < mediaItems.length) {
        return currentIndex
      }

      return defaultMediaIndex
    })
  }, [defaultMediaIndex, mediaItems.length])

  useEffect(() => {
    setIsImageZoomed(false)
    setImageZoomOrigin({ x: 50, y: 50 })
  }, [resolvedMediaIndex])

  function handleVariantOptionChange(attributeDefinitionId: string, nextValue: string) {
    const targetSelections = new Map(selectedAttributeValues)
    targetSelections.set(attributeDefinitionId, nextValue)

    const exactMatch = product.variants.find((variant) =>
      variantAttributeDefinitions.every((attribute) => {
        return (
          getVariantAttributeValue(variant, attribute.attributeDefinitionId) ===
          targetSelections.get(attribute.attributeDefinitionId)
        )
      }),
    )

    if (exactMatch) {
      setSelectedVariantId(exactMatch.id)
      return
    }

    const fallbackMatch = product.variants
      .map((variant) => {
        if (getVariantAttributeValue(variant, attributeDefinitionId) !== nextValue) {
          return null
        }

        const score = variantAttributeDefinitions.reduce((matchCount, attribute) => {
          if (attribute.attributeDefinitionId === attributeDefinitionId) {
            return matchCount
          }

          return (
            matchCount +
            (getVariantAttributeValue(variant, attribute.attributeDefinitionId) ===
            targetSelections.get(attribute.attributeDefinitionId)
              ? 1
              : 0)
          )
        }, 0)

        return { variant, score }
      })
      .filter((item): item is { variant: StorefrontVariant; score: number } => item !== null)
      .sort((left, right) => right.score - left.score)[0]?.variant

    if (fallbackMatch) {
      setSelectedVariantId(fallbackMatch.id)
    }
  }

  const handleAddToCart = () => {
    if (!cartProduct) {
      return
    }

    addToCart(cartProduct, quantity)
  }

  const incrementQuantity = () => setQuantity((current) => current + 1)
  const decrementQuantity = () =>
    setQuantity((current) => (current > 1 ? current - 1 : current))

  const handleMainImagePointerMove = (
    event: MouseEvent<HTMLDivElement> | TouchEvent<HTMLDivElement>,
  ) => {
    const currentTarget = event.currentTarget
    const rect = currentTarget.getBoundingClientRect()

    let clientX = 0
    let clientY = 0

    if ("touches" in event) {
      const touch = event.touches[0]
      if (!touch) {
        return
      }

      clientX = touch.clientX
      clientY = touch.clientY
    } else {
      clientX = event.clientX
      clientY = event.clientY
    }

    const x = ((clientX - rect.left) / rect.width) * 100
    const y = ((clientY - rect.top) / rect.height) * 100

    setImageZoomOrigin({
      x: Math.max(0, Math.min(100, x)),
      y: Math.max(0, Math.min(100, y)),
    })
  }

  return (
    <div className="mx-auto max-w-7xl px-6 lg:px-8">
      <nav className="mb-8 lg:mb-10">
        <Link
          href={storefrontPath(storeSlug)}
          className="premium-link inline-flex items-center gap-3 text-[11px] uppercase tracking-[0.25em] text-muted-foreground transition-colors hover:text-foreground"
        >
          <ChevronLeft className="h-4 w-4" strokeWidth={1} />
          Back to Storefront
        </Link>
      </nav>

      <div className="grid gap-16 lg:grid-cols-2 lg:gap-24">
        <div className="space-y-6">
          <div
            className="group relative aspect-[3/4] overflow-hidden bg-secondary"
            onMouseMove={
              mainMediaItem?.mediaType === "Video" ? undefined : handleMainImagePointerMove
            }
            onMouseEnter={
              mainMediaItem?.mediaType === "Video" ? undefined : () => setIsImageZoomed(true)
            }
            onMouseLeave={
              mainMediaItem?.mediaType === "Video"
                ? undefined
                : () => setIsImageZoomed(false)
            }
          >
            {mainMediaItem?.mediaType === "Video" ? (
              <video
                key={mainMediaItem.id}
                src={mainMediaItem.url}
                controls
                playsInline
                preload="metadata"
                className="h-full w-full object-cover"
              />
            ) : (
              <Image
                key={mainMediaItem?.id ?? "product-media"}
                src={mainMediaItem?.url ?? "/placeholder.jpg"}
                alt={mainMediaItem?.altText || product.name}
                fill
                className="cursor-zoom-in object-cover transition-[transform,filter] duration-700 ease-[cubic-bezier(0.22,1,0.36,1)] will-change-transform"
                style={{
                  transformOrigin: `${imageZoomOrigin.x}% ${imageZoomOrigin.y}%`,
                  transform: isImageZoomed ? "scale(1.55)" : "scale(1)",
                  filter: isImageZoomed ? "saturate(1.02)" : "saturate(1)",
                }}
                priority
              />
            )}

            {resolvedPrice?.isOnSale ? (
              <span className="absolute left-0 top-8 bg-foreground px-6 py-2 text-[10px] font-normal uppercase tracking-[0.2em] text-background">
                {saleDiscountPercentage ? `On Sale - ${saleDiscountPercentage}% Off` : "On Sale"}
              </span>
            ) : null}
          </div>

          {mediaItems.length > 1 ? (
            <div className="grid grid-cols-4 gap-3 sm:grid-cols-5 lg:grid-cols-4 xl:grid-cols-5">
              {mediaItems.map((item, index) => (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => setSelectedMediaIndex(index)}
                  className={cn(
                    "relative aspect-square overflow-hidden border bg-secondary transition-colors",
                    resolvedMediaIndex === index
                      ? "border-foreground"
                      : "border-transparent hover:border-border",
                  )}
                >
                  {item.mediaType === "Video" ? (
                    <video
                      src={item.url}
                      muted
                      playsInline
                      preload="metadata"
                      className="h-full w-full object-cover"
                    />
                  ) : (
                    <Image
                      src={item.url}
                      alt={item.altText || product.name}
                      fill
                      className="object-cover"
                    />
                  )}
                </button>
              ))}
            </div>
          ) : null}
        </div>

        <div className="flex flex-col lg:py-8">
          <p className="text-[10px] font-normal uppercase tracking-[0.4em] text-muted-foreground">
            {product.brandName || product.productType}
          </p>
          <h1 className="mt-4 font-serif text-4xl font-light tracking-tight text-foreground lg:text-5xl">
            {product.name}
          </h1>

          <div className="mt-8 flex items-baseline gap-4">
            {resolvedPrice ? (
              <>
                <span className="text-2xl tracking-wide text-foreground">
                  {formatMoney(resolvedPrice.amount, resolvedPrice.currencyCode)}
                </span>
                {resolvedPrice.compareAtAmount ? (
                  <span className="text-lg text-muted-foreground line-through">
                    {formatMoney(
                      resolvedPrice.compareAtAmount,
                      resolvedPrice.currencyCode,
                    )}
                  </span>
                ) : null}
              </>
            ) : (
              <span className="text-lg text-muted-foreground">Price unavailable</span>
            )}
          </div>

          <div className="my-8 h-px w-16 bg-border" />

          {product.shortDescription ? (
            <p className="text-base leading-relaxed text-foreground/85">
              {product.shortDescription}
            </p>
          ) : null}

          {product.description ? (
            <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
              {product.description}
            </p>
          ) : null}

          {product.variants.length > 0 && variantAttributeDefinitions.length > 0 ? (
            <div className="mt-10">
              <div className="space-y-8">
                {variantAttributeDefinitions.map((attribute) => {
                  const options = Array.from(
                    new Set(
                      product.variants
                        .map((variant) =>
                          getVariantAttributeValue(variant, attribute.attributeDefinitionId),
                        )
                        .filter((value): value is string => Boolean(value)),
                    ),
                  )

                  return (
                    <div key={attribute.attributeDefinitionId}>
                      <h3 className="text-[10px] font-normal uppercase tracking-[0.3em] text-foreground">
                        {attribute.name}
                      </h3>
                      <div className="mt-4 flex flex-wrap gap-3">
                        {options.map((optionValue) => {
                          const isSelected =
                            selectedAttributeValues.get(attribute.attributeDefinitionId) ===
                            optionValue
                          const isAvailable = product.variants.some((variant) => {
                            if (
                              getVariantAttributeValue(
                                variant,
                                attribute.attributeDefinitionId,
                              ) !== optionValue
                            ) {
                              return false
                            }

                            return variantAttributeDefinitions.every((otherAttribute) => {
                              if (
                                otherAttribute.attributeDefinitionId ===
                                attribute.attributeDefinitionId
                              ) {
                                return true
                              }

                              const selectedValue = selectedAttributeValues.get(
                                otherAttribute.attributeDefinitionId,
                              )

                              if (!selectedValue) {
                                return true
                              }

                              return (
                                getVariantAttributeValue(
                                  variant,
                                  otherAttribute.attributeDefinitionId,
                                ) === selectedValue
                              )
                            })
                          })

                          const optionLabel = resolveVariantOptionLabel(
                            attribute,
                            optionValue,
                            product.variants,
                          )
                          const showColorSwatch =
                            (normalizeAttributeCode(attribute.code) === "color" ||
                              normalizeAttributeCode(attribute.code) === "colour") &&
                            isHexColor(optionValue)

                          return (
                            <button
                              key={`${attribute.attributeDefinitionId}-${optionValue}`}
                              type="button"
                              disabled={!isAvailable}
                              onClick={() =>
                                handleVariantOptionChange(
                                  attribute.attributeDefinitionId,
                                  optionValue,
                                )
                              }
                              className={cn(
                                "border px-6 py-3 text-[11px] uppercase tracking-[0.15em] transition-all duration-300",
                                isSelected
                                  ? "border-foreground bg-foreground text-background"
                                  : "border-border bg-transparent text-foreground hover:border-foreground",
                                !isAvailable && "cursor-not-allowed opacity-35 hover:border-border",
                                showColorSwatch && "inline-flex items-center gap-3",
                              )}
                            >
                              {showColorSwatch ? (
                                <span
                                  className="h-3 w-3 border border-current"
                                  style={{ backgroundColor: optionValue }}
                                  aria-hidden="true"
                                />
                              ) : null}
                              {optionLabel}
                            </button>
                          )
                        })}
                      </div>
                    </div>
                  )
                })}
              </div>
            </div>
          ) : null}

          {selectedVariant?.attributes.length ? (
            <div className="mt-8">
              <h3 className="text-[10px] font-normal uppercase tracking-[0.3em] text-foreground">
                Selected Attributes
              </h3>
              <div className="mt-4 flex flex-wrap gap-3">
                {selectedVariant.attributes.map((attribute) => (
                  <span
                    key={`${selectedVariant.id}-${attribute.attributeDefinitionId}`}
                    className="border border-border px-4 py-2 text-[11px] uppercase tracking-[0.15em] text-muted-foreground"
                  >
                    {attribute.name}: {attribute.value}
                  </span>
                ))}
              </div>
            </div>
          ) : null}

          {product.attributes.length ? (
            <div className="mt-8">
              <h3 className="text-[10px] font-normal uppercase tracking-[0.3em] text-foreground">
                Product Details
              </h3>
              <div className="mt-4 flex flex-wrap gap-3">
                {product.attributes
                  .filter((attribute) => !attribute.isVariantDefining)
                  .map((attribute) => (
                    <span
                      key={attribute.attributeDefinitionId}
                      className="border border-border px-4 py-2 text-[11px] uppercase tracking-[0.15em] text-muted-foreground"
                    >
                      {attribute.name}: {attribute.value}
                    </span>
                  ))}
              </div>
            </div>
          ) : null}

          <div className="mt-12 flex flex-col gap-4 sm:flex-row sm:flex-wrap sm:items-center">
            <div className="flex w-full items-center border border-border sm:w-auto">
              <button
                onClick={decrementQuantity}
                className="flex h-14 flex-1 items-center justify-center text-muted-foreground transition-colors hover:text-foreground sm:w-14 sm:flex-none"
              >
                <Minus className="h-4 w-4" strokeWidth={1} />
              </button>
              <span className="flex h-14 min-w-0 flex-1 items-center justify-center text-sm font-light text-foreground sm:w-16 sm:flex-none">
                {quantity}
              </span>
              <button
                onClick={incrementQuantity}
                className="flex h-14 flex-1 items-center justify-center text-muted-foreground transition-colors hover:text-foreground sm:w-14 sm:flex-none"
              >
                <Plus className="h-4 w-4" strokeWidth={1} />
              </button>
            </div>

            <Button
              onClick={handleAddToCart}
              size="lg"
              className="h-14 w-full flex-1 text-[11px] uppercase tracking-[0.2em] sm:w-auto"
              disabled={!cartProduct}
            >
              <ShoppingBag className="mr-3 h-4 w-4" strokeWidth={1} />
              Add to Cart
            </Button>

            <Button
              variant="outline"
              size="icon"
              className="h-14 w-full border-border hover:border-foreground hover:bg-transparent sm:w-14"
            >
              <Heart className="h-5 w-5" strokeWidth={1} />
              <span className="sr-only">Wishlist</span>
            </Button>
          </div>

          <div className="mt-16 grid gap-6 border-t border-border pt-12 sm:grid-cols-3">
            <div className="flex flex-col items-center text-center">
              <Truck className="h-5 w-5 text-foreground" strokeWidth={1} />
              <span className="mt-3 text-[10px] uppercase tracking-[0.2em] text-foreground">
                Shipment Tracking
              </span>
              <span className="mt-1 text-xs text-muted-foreground">
                Order-based delivery updates
              </span>
            </div>
            <div className="flex flex-col items-center text-center">
              <Shield className="h-5 w-5 text-foreground" strokeWidth={1} />
              <span className="mt-3 text-[10px] uppercase tracking-[0.2em] text-foreground">
                Secure Payment
              </span>
              <span className="mt-1 text-xs text-muted-foreground">
                Hosted authorization flow
              </span>
            </div>
            <div className="flex flex-col items-center text-center">
              <RotateCcw className="h-5 w-5 text-foreground" strokeWidth={1} />
              <span className="mt-3 text-[10px] uppercase tracking-[0.2em] text-foreground">
                Account Support
              </span>
              <span className="mt-1 text-xs text-muted-foreground">
                Orders and addresses in one place
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
