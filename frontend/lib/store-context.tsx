"use client"

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react"
import { usePathname } from "next/navigation"
import type { CartProductInput } from "@/lib/storefront-adapters"

const storageKeyPrefix = "modular-ecommerce-cart"
const reservedRouteSegments = new Set([
  "about",
  "account",
  "admin",
  "api",
  "auth",
  "brands",
  "cart",
  "categories",
  "checkout",
  "contact",
  "help",
  "order-success",
  "payment-result",
  "privacy-policy",
  "product",
  "products",
  "return-policy",
  "shipping-policy",
  "store-unavailable",
  "terms",
  "unauthorized",
])

export interface CartItem extends CartProductInput {
  quantity: number
}

interface StoreContextValue {
  cart: CartItem[]
  addToCart: (product: CartProductInput, quantity?: number) => void
  removeFromCart: (productId: string, variantId?: string | null) => void
  updateQuantity: (
    productId: string,
    quantity: number,
    variantId?: string | null,
  ) => void
  clearCart: () => void
  getCartTotal: () => number
  getCartCount: () => number
}

const StoreContext = createContext<StoreContextValue | undefined>(undefined)

function resolveStoreSlugFromPathname(pathname: string | null): string | null {
  if (!pathname) {
    return null
  }

  const [firstSegment] = pathname.split("/").filter(Boolean)

  if (!firstSegment || reservedRouteSegments.has(firstSegment.toLowerCase())) {
    return null
  }

  return firstSegment
}

function getStorageKey(storeSlug: string | null): string {
  if (!storeSlug) {
    return storageKeyPrefix
  }

  return `${storageKeyPrefix}:${storeSlug.toLowerCase()}`
}

function isSameLine(
  item: CartItem,
  productId: string,
  variantId?: string | null,
): boolean {
  return item.productId === productId && (item.variantId ?? null) === (variantId ?? null)
}

export function StoreProvider({ children }: { children: ReactNode }) {
  const pathname = usePathname()
  const activeStoreSlug = useMemo(
    () => resolveStoreSlugFromPathname(pathname),
    [pathname],
  )
  const activeStorageKey = useMemo(
    () => getStorageKey(activeStoreSlug),
    [activeStoreSlug],
  )
  const [cartsByKey, setCartsByKey] = useState<Record<string, CartItem[]>>({})
  const [loadedKeys, setLoadedKeys] = useState<Record<string, true>>({})

  const cart = cartsByKey[activeStorageKey] ?? []

  useEffect(() => {
    if (typeof window === "undefined") {
      return
    }

    if (loadedKeys[activeStorageKey]) {
      return
    }

    const raw = window.localStorage.getItem(activeStorageKey)

    let parsed: CartItem[] = []

    if (raw) {
      try {
        const candidate = JSON.parse(raw) as CartItem[]
        parsed = Array.isArray(candidate) ? candidate : []
      } catch {
        parsed = []
      }
    }

    setCartsByKey((current) => ({
      ...current,
      [activeStorageKey]: parsed,
    }))
    setLoadedKeys((current) => ({
      ...current,
      [activeStorageKey]: true,
    }))
  }, [activeStorageKey, loadedKeys])

  useEffect(() => {
    if (typeof window === "undefined") {
      return
    }

    if (!loadedKeys[activeStorageKey]) {
      return
    }

    window.localStorage.setItem(activeStorageKey, JSON.stringify(cart))
  }, [activeStorageKey, cart, loadedKeys])

  const addToCart = useCallback((product: CartProductInput, quantity = 1) => {
    setCartsByKey((current) => {
      const activeCart = current[activeStorageKey] ?? []
      const existing = activeCart.find((item) =>
        isSameLine(item, product.productId, product.variantId),
      )

      if (!existing) {
        return {
          ...current,
          [activeStorageKey]: [...activeCart, { ...product, quantity }],
        }
      }

      return {
        ...current,
        [activeStorageKey]: activeCart.map((item) =>
          isSameLine(item, product.productId, product.variantId)
            ? { ...item, quantity: item.quantity + quantity }
            : item,
        ),
      }
    })
  }, [activeStorageKey])

  const removeFromCart = useCallback(
    (productId: string, variantId?: string | null) => {
      setCartsByKey((current) => {
        const activeCart = current[activeStorageKey] ?? []

        return {
          ...current,
          [activeStorageKey]: activeCart.filter(
            (item) => !isSameLine(item, productId, variantId),
          ),
        }
      })
    },
    [activeStorageKey],
  )

  const updateQuantity = useCallback(
    (productId: string, quantity: number, variantId?: string | null) => {
      if (quantity <= 0) {
        removeFromCart(productId, variantId)
        return
      }

      setCartsByKey((current) => {
        const activeCart = current[activeStorageKey] ?? []

        return {
          ...current,
          [activeStorageKey]: activeCart.map((item) =>
            isSameLine(item, productId, variantId) ? { ...item, quantity } : item,
          ),
        }
      })
    },
    [activeStorageKey, removeFromCart],
  )

  const clearCart = useCallback(() => {
    setCartsByKey((current) => ({
      ...current,
      [activeStorageKey]: [],
    }))
  }, [activeStorageKey])

  const value = useMemo<StoreContextValue>(
    () => ({
      cart,
      addToCart,
      removeFromCart,
      updateQuantity,
      clearCart,
      getCartTotal: () =>
        cart.reduce((total, item) => total + item.priceAmount * item.quantity, 0),
      getCartCount: () => cart.reduce((count, item) => count + item.quantity, 0),
    }),
    [addToCart, cart, clearCart, removeFromCart, updateQuantity],
  )

  return <StoreContext.Provider value={value}>{children}</StoreContext.Provider>
}

export function useStore() {
  const context = useContext(StoreContext)

  if (!context) {
    throw new Error("useStore must be used within a StoreProvider")
  }

  return context
}
