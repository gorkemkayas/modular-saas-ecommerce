"use client"

import { useEffect, useState, useTransition } from "react"
import { useRouter } from "next/navigation"

import {
  activatePriceEntry,
  deactivatePriceEntry,
  getProductById,
  removeProductPrice,
  removeVariantPrice,
  searchProducts,
  setProductPrice,
  setVariantPrice,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import type {
  AdminProductDto,
  PriceEntryDto,
  ProductSummaryDto,
} from "@/lib/api/types"

interface PriceEntryDisplayItem {
  entry: PriceEntryDto
  productName: string
  productSlug: string
  variantName: string | null
  variantSku: string | null
}

function PriceEntryRowActions({
  priceListId,
  item,
}: {
  priceListId: string
  item: PriceEntryDisplayItem
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [error, setError] = useState<string | null>(null)

  function run(action: () => Promise<void>) {
    setError(null)
    startTransition(async () => {
      try {
        await action()
        router.refresh()
      } catch (actionError) {
        setError(getApiErrorMessage(actionError, "The price entry action failed."))
      }
    })
  }

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap justify-end gap-2">
        {item.entry.isActive ? (
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => deactivatePriceEntry(priceListId, item.entry.id))}
            className="border border-border px-3 py-2 text-xs transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Deactivate
          </button>
        ) : (
          <button
            type="button"
            disabled={isPending}
            onClick={() => run(() => activatePriceEntry(priceListId, item.entry.id))}
            className="border border-border px-3 py-2 text-xs transition-colors hover:bg-secondary disabled:opacity-60"
          >
            Activate
          </button>
        )}
        <button
          type="button"
          disabled={isPending}
          onClick={() =>
            run(() =>
              item.entry.productVariantId
                ? removeVariantPrice(
                    priceListId,
                    item.entry.productId,
                    item.entry.productVariantId,
                  )
                : removeProductPrice(priceListId, item.entry.productId),
            )
          }
          className="border border-destructive/30 px-3 py-2 text-xs text-destructive transition-colors hover:bg-destructive/5 disabled:opacity-60"
        >
          Remove
        </button>
      </div>
      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-3 py-2 text-xs text-destructive">
          {error}
        </div>
      ) : null}
    </div>
  )
}

export function AdminPriceEntryManager({
  priceListId,
  currencyCode,
  entries,
}: {
  priceListId: string
  currencyCode: string
  entries: PriceEntryDisplayItem[]
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [searchTerm, setSearchTerm] = useState("")
  const [productOptions, setProductOptions] = useState<ProductSummaryDto[]>([])
  const [selectedProductId, setSelectedProductId] = useState("")
  const [selectedVariantId, setSelectedVariantId] = useState("")
  const [selectedProduct, setSelectedProduct] = useState<AdminProductDto | null>(null)
  const [amount, setAmount] = useState("")
  const [compareAtAmount, setCompareAtAmount] = useState("")
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    const loadProducts = async () => {
      try {
        const result = await searchProducts({
          searchTerm: searchTerm.trim() || undefined,
          pageNumber: 1,
          pageSize: 50,
        })

        if (!cancelled) {
          setProductOptions(result.items)
        }
      } catch {
        if (!cancelled) {
          setProductOptions([])
        }
      }
    }

    void loadProducts()

    return () => {
      cancelled = true
    }
  }, [searchTerm])

  useEffect(() => {
    let cancelled = false

    const loadProduct = async () => {
      if (!selectedProductId) {
        setSelectedProduct(null)
        setSelectedVariantId("")
        return
      }

      try {
        const product = await getProductById(selectedProductId)
        if (!cancelled) {
          setSelectedProduct(product)
          setSelectedVariantId("")
        }
      } catch (loadError) {
        if (!cancelled) {
          setSelectedProduct(null)
          setError(getApiErrorMessage(loadError, "The product details could not be loaded."))
        }
      }
    }

    void loadProduct()

    return () => {
      cancelled = true
    }
  }, [selectedProductId])

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setMessage(null)

    if (!selectedProductId) {
      setError("Select a product first.")
      return
    }

    if (!amount.trim()) {
      setError("Price amount is required.")
      return
    }

    startTransition(async () => {
      try {
        if (selectedVariantId) {
          await setVariantPrice(priceListId, selectedProductId, selectedVariantId, {
            amount: Number(amount),
            compareAtAmount: compareAtAmount.trim() ? Number(compareAtAmount) : null,
          })
        } else {
          await setProductPrice(priceListId, selectedProductId, {
            amount: Number(amount),
            compareAtAmount: compareAtAmount.trim() ? Number(compareAtAmount) : null,
          })
        }

        setMessage("Price entry saved.")
        setAmount("")
        setCompareAtAmount("")
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "The price entry could not be saved."))
      }
    })
  }

  return (
    <div className="space-y-6">
      <div className="border border-border p-6">
        <h2 className="text-lg font-light tracking-wide">Add Price Entry</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          Product prices are assigned here after the catalog item exists.
        </p>

        <form onSubmit={handleSubmit} className="mt-6 space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="block text-sm">Search Products</label>
              <input
                value={searchTerm}
                onChange={(event) => setSearchTerm(event.target.value)}
                placeholder="Search by name or slug"
                className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
              />
            </div>
            <div className="space-y-2">
              <label className="block text-sm">Product</label>
              <select
                value={selectedProductId}
                onChange={(event) => setSelectedProductId(event.target.value)}
                className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
              >
                <option value="">Select product</option>
                {productOptions.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.name} / {product.slug}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {selectedProduct?.productType === "Variant" ? (
            <div className="space-y-2">
              <label className="block text-sm">Variant</label>
              <select
                value={selectedVariantId}
                onChange={(event) => setSelectedVariantId(event.target.value)}
                className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
              >
                <option value="">Product-level price</option>
                {selectedProduct.variants.map((variant) => (
                  <option key={variant.id} value={variant.id}>
                    {variant.name ?? variant.sku} / {variant.sku}
                  </option>
                ))}
              </select>
            </div>
          ) : null}

          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="block text-sm">Amount ({currencyCode})</label>
              <input
                type="number"
                step="0.01"
                value={amount}
                onChange={(event) => setAmount(event.target.value)}
                className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
              />
            </div>
            <div className="space-y-2">
              <label className="block text-sm">Compare At ({currencyCode})</label>
              <input
                type="number"
                step="0.01"
                value={compareAtAmount}
                onChange={(event) => setCompareAtAmount(event.target.value)}
                className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={isPending}
            className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
          >
            {isPending ? "Saving..." : "Save Price"}
          </button>
        </form>

        {message ? (
          <div className="mt-4 border border-border bg-secondary/30 px-4 py-3 text-sm">
            {message}
          </div>
        ) : null}

        {error ? (
          <div className="mt-4 border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
            {error}
          </div>
        ) : null}
      </div>

      <div className="border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Product</th>
                <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Variant</th>
                <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Amount</th>
                <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">Compare At</th>
                <th className="px-6 py-4 text-left text-xs uppercase tracking-wider text-muted-foreground">State</th>
                <th className="px-6 py-4 text-right text-xs uppercase tracking-wider text-muted-foreground">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {entries.map((item) => (
                <tr key={item.entry.id} className="hover:bg-secondary/30">
                  <td className="px-6 py-4 text-sm">
                    <div>
                      <p className="font-medium">{item.productName}</p>
                      <p className="text-xs text-muted-foreground">/{item.productSlug}</p>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-muted-foreground">
                    {item.variantName ?? item.variantSku ?? "Product-level price"}
                  </td>
                  <td className="px-6 py-4 text-sm">
                    {item.entry.amount} {item.entry.currencyCode}
                  </td>
                  <td className="px-6 py-4 text-sm text-muted-foreground">
                    {item.entry.compareAtAmount !== null
                      ? `${item.entry.compareAtAmount} ${item.entry.currencyCode}`
                      : "Not set"}
                  </td>
                  <td className="px-6 py-4 text-sm">
                    {item.entry.isActive ? "Active" : "Inactive"}
                  </td>
                  <td className="px-6 py-4 text-right">
                    <PriceEntryRowActions priceListId={priceListId} item={item} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
