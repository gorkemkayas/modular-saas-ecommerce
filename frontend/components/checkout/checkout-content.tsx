"use client"

import { useEffect, useMemo, useState } from "react"
import Image from "next/image"
import Link from "next/link"
import { ChevronLeft, CreditCard, Loader2, Lock, Truck } from "lucide-react"
import { Button } from "@/components/ui/button"
import { getAuthSession, isSessionForStore } from "@/lib/api/auth"
import {
  getMyProfile,
  getStorefrontShippingCarriers,
  placeOrder,
  createOrderPayment,
  authorizeOrderPayment,
} from "@/lib/api/account"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { getAccountPath } from "@/lib/account-path"
import type { CustomerAddressDto, ShippingCarrierDto } from "@/lib/api/types"
import { storefrontPath } from "@/lib/config"
import { formatMoney } from "@/lib/format"
import { useStore } from "@/lib/store-context"

type Step = "addresses" | "payment"
type PaymentMethod = "Card" | "BankTransfer" | "CashOnDelivery"

interface CheckoutContentProps {
  storeSlug: string
}

export function CheckoutContent({ storeSlug }: CheckoutContentProps) {
  const { cart, getCartTotal, clearCart } = useStore()
  const [currentStep, setCurrentStep] = useState<Step>("addresses")
  const [addresses, setAddresses] = useState<CustomerAddressDto[]>([])
  const [shippingCarriers, setShippingCarriers] = useState<ShippingCarrierDto[]>([])
  const [shippingAddressId, setShippingAddressId] = useState<string>("")
  const [billingAddressId, setBillingAddressId] = useState<string>("")
  const [shippingCarrierId, setShippingCarrierId] = useState<string>("")
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("Card")
  const [isLoadingProfile, setIsLoadingProfile] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let isMounted = true

    const loadProfile = async () => {
      try {
        const session = await getAuthSession()

        if (!isSessionForStore(session, storeSlug)) {
          if (isMounted) {
            setAddresses([])
            setError("Please sign in to this store before continuing to checkout.")
          }

          return
        }

        const [profile, carriers] = await Promise.all([
          getMyProfile(),
          getStorefrontShippingCarriers(storeSlug),
        ])

        if (!isMounted) {
          return
        }

        setAddresses(profile.addresses)
        setShippingCarriers(carriers)
        setShippingAddressId(
          profile.addresses.find((item) => item.isDefaultShipping)?.id ??
            profile.addresses[0]?.id ??
            "",
        )
        setBillingAddressId(
          profile.addresses.find((item) => item.isDefaultBilling)?.id ??
            profile.addresses[0]?.id ??
            "",
        )
        setShippingCarrierId(carriers[0]?.id ?? "")

        if (!carriers.length) {
          setError("No shipping carrier is available for this store.")
        }
      } catch (loadError) {
        if (isMounted) {
          setError("We couldn't load your customer profile. Please sign in again.")
        }
      } finally {
        if (isMounted) {
          setIsLoadingProfile(false)
        }
      }
    }

    void loadProfile()

    return () => {
      isMounted = false
    }
  }, [storeSlug])

  const subtotal = getCartTotal()
  const currencyCode = cart[0]?.currencyCode ?? "TRY"
  const total = subtotal
  const accountAddressesHref = getAccountPath(storeSlug, "/addresses")

  const selectedShipping = useMemo(
    () => addresses.find((item) => item.id === shippingAddressId) ?? null,
    [addresses, shippingAddressId],
  )
  const selectedBilling = useMemo(
    () => addresses.find((item) => item.id === billingAddressId) ?? null,
    [addresses, billingAddressId],
  )
  const selectedCarrier = useMemo(
    () => shippingCarriers.find((item) => item.id === shippingCarrierId) ?? null,
    [shippingCarriers, shippingCarrierId],
  )

  if (!cart.length) {
    return (
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center py-32 text-center">
          <h1 className="font-serif text-4xl font-light text-foreground">
            Your cart is empty
          </h1>
          <p className="mt-4 text-sm text-muted-foreground">
            Add at least one product before placing an order.
          </p>
          <Button asChild className="mt-10 h-14 px-12 text-[11px] uppercase tracking-[0.2em]">
            <Link href={storefrontPath(storeSlug, "/products")}>Browse Catalog</Link>
          </Button>
        </div>
      </div>
    )
  }

  const handlePlaceOrder = async () => {
    if (!shippingAddressId || !billingAddressId) {
      setError("Please choose both shipping and billing addresses.")
      return
    }

    if (!shippingCarrierId) {
      setError("Please choose a shipping carrier.")
      return
    }

    setError(null)
    setIsSubmitting(true)

    try {
      const orderResponse = await placeOrder({
        shippingAddressId,
        billingAddressId,
        shippingCarrierId,
        currencyCode,
        items: cart.map((item) => ({
          productId: item.productId,
          productVariantId: item.variantId,
          quantity: item.quantity,
        })),
      })

      await createOrderPayment(orderResponse.orderId, {
        methodType: paymentMethod,
      })

      const authorization = await authorizeOrderPayment(orderResponse.orderId, {
        idempotencyKey: crypto.randomUUID(),
      })

      if (authorization.actionUrl) {
        window.location.href = authorization.actionUrl
        return
      }

      clearCart()

      if (authorization.status === "Failed") {
        window.location.href = `${storefrontPath(
          storeSlug,
          "/payment-result",
        )}?status=failed&orderId=${orderResponse.orderId}`
        return
      }

      window.location.href = `${storefrontPath(
        storeSlug,
        "/payment-result",
      )}?status=success&orderId=${orderResponse.orderId}`
    } catch (checkoutError) {
      setError(
        getApiErrorMessage(
          checkoutError,
          "We couldn't complete checkout. Please review your details and try again.",
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-7xl px-6 lg:px-8">
      <nav className="mb-16">
        <Link
          href={storefrontPath(storeSlug, "/cart")}
          className="premium-link inline-flex items-center gap-3 text-[11px] uppercase tracking-[0.25em] text-muted-foreground transition-colors hover:text-foreground"
        >
          <ChevronLeft className="h-4 w-4" strokeWidth={1} />
          Back to Cart
        </Link>
      </nav>

      <div className="mb-20">
        <div className="flex items-center justify-center">
          {[
            { id: "addresses", name: "Addresses", icon: Truck },
            { id: "payment", name: "Payment", icon: CreditCard },
          ].map((step, index) => {
            const isActive = currentStep === step.id
            const isDone = currentStep === "payment" && step.id === "addresses"

            return (
              <div key={step.id} className="flex items-center">
                <div
                  className={`flex items-center gap-3 px-8 py-4 text-[10px] uppercase tracking-[0.2em] ${
                    isActive || isDone
                      ? "bg-foreground text-background"
                      : "bg-secondary text-muted-foreground"
                  }`}
                >
                  <step.icon className="h-4 w-4" strokeWidth={1} />
                  <span className="hidden sm:inline">{step.name}</span>
                </div>
                {index < 1 ? (
                  <div className={`h-px w-12 sm:w-20 ${isDone ? "bg-foreground" : "bg-border"}`} />
                ) : null}
              </div>
            )
          })}
        </div>
      </div>

      <div className="grid gap-16 lg:grid-cols-3 lg:gap-24">
        <div className="lg:col-span-2">
          {isLoadingProfile ? (
            <div className="flex items-center gap-3 text-sm text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" />
              Loading customer profile...
            </div>
          ) : (
            <>
              {currentStep === "addresses" ? (
                <div>
                  <h2 className="font-serif text-3xl font-light text-foreground">
                    Address Selection
                  </h2>
                  <p className="mt-4 text-sm text-muted-foreground">
                    Checkout uses the addresses saved under your customer account.
                  </p>

                  {!addresses.length ? (
                    <div className="mt-8 border border-border p-6">
                      <p className="text-sm text-muted-foreground">
                        No address found in your account.
                      </p>
                      <Link
                        href={accountAddressesHref}
                        className="mt-4 inline-block text-sm underline underline-offset-4"
                      >
                        Add an address in My Account
                      </Link>
                    </div>
                  ) : (
                    <div className="mt-10 grid gap-8">
                      <div>
                        <h3 className="text-[10px] uppercase tracking-[0.2em] text-foreground">
                          Shipping Address
                        </h3>
                        <div className="mt-4 grid gap-3">
                          {addresses.map((address) => (
                            <label
                              key={`shipping-${address.id}`}
                              className={`block cursor-pointer border p-4 transition-colors ${
                                shippingAddressId === address.id
                                  ? "border-foreground bg-secondary/50"
                                  : "border-border"
                              }`}
                            >
                              <input
                                type="radio"
                                name="shippingAddress"
                                value={address.id}
                                checked={shippingAddressId === address.id}
                                onChange={() => setShippingAddressId(address.id)}
                                className="sr-only"
                              />
                              <p className="font-medium">{address.title}</p>
                              <p className="mt-1 text-sm text-muted-foreground">
                                {address.contactName}
                              </p>
                              <p className="text-sm text-muted-foreground">
                                {address.line1}, {address.district}, {address.city}
                              </p>
                            </label>
                          ))}
                        </div>
                      </div>

                      <div>
                        <h3 className="text-[10px] uppercase tracking-[0.2em] text-foreground">
                          Billing Address
                        </h3>
                        <div className="mt-4 grid gap-3">
                          {addresses.map((address) => (
                            <label
                              key={`billing-${address.id}`}
                              className={`block cursor-pointer border p-4 transition-colors ${
                                billingAddressId === address.id
                                  ? "border-foreground bg-secondary/50"
                                  : "border-border"
                              }`}
                            >
                              <input
                                type="radio"
                                name="billingAddress"
                                value={address.id}
                                checked={billingAddressId === address.id}
                                onChange={() => setBillingAddressId(address.id)}
                                className="sr-only"
                              />
                              <p className="font-medium">{address.title}</p>
                              <p className="mt-1 text-sm text-muted-foreground">
                                {address.contactName}
                              </p>
                              <p className="text-sm text-muted-foreground">
                                {address.line1}, {address.district}, {address.city}
                              </p>
                            </label>
                          ))}
                        </div>
                      </div>

                      <div>
                        <h3 className="text-[10px] uppercase tracking-[0.2em] text-foreground">
                          Shipping Carrier
                        </h3>
                        <div className="mt-4 grid gap-3 sm:grid-cols-2">
                          {shippingCarriers.map((carrier) => (
                            <label
                              key={carrier.id}
                              className={`block cursor-pointer border p-4 transition-colors ${
                                shippingCarrierId === carrier.id
                                  ? "border-foreground bg-secondary/50"
                                  : "border-border"
                              }`}
                            >
                              <input
                                type="radio"
                                name="shippingCarrier"
                                value={carrier.id}
                                checked={shippingCarrierId === carrier.id}
                                onChange={() => setShippingCarrierId(carrier.id)}
                                className="sr-only"
                              />
                              <p className="font-medium">{carrier.name}</p>
                              <p className="mt-1 text-[10px] uppercase tracking-[0.2em] text-muted-foreground">
                                {carrier.serviceName || carrier.serviceCode || carrier.code}
                              </p>
                            </label>
                          ))}
                        </div>
                        {!shippingCarriers.length ? (
                          <div className="mt-4 border border-border p-4 text-sm text-muted-foreground">
                            Shipping carrier selection is not available.
                          </div>
                        ) : null}
                      </div>
                    </div>
                  )}

                  <div className="mt-12 flex justify-end">
                    <Button
                      type="button"
                      className="h-14 px-12 text-[11px] uppercase tracking-[0.2em]"
                      disabled={!addresses.length || !shippingCarrierId}
                      onClick={() => setCurrentStep("payment")}
                    >
                      Continue to Payment
                    </Button>
                  </div>
                </div>
              ) : (
                <div>
                  <h2 className="font-serif text-3xl font-light text-foreground">
                    Payment Method
                  </h2>
                  <div className="mt-4 flex items-center gap-2 text-xs text-muted-foreground">
                    <Lock className="h-3 w-3" strokeWidth={1} />
                    <span>Payment authorization is handled by the backend provider flow.</span>
                  </div>

                  <div className="mt-10 space-y-3">
                    {[
                      {
                        id: "Card",
                        label: "Card",
                        description: "Creates the payment and authorizes through the provider flow.",
                      },
                      {
                        id: "BankTransfer",
                        label: "Bank Transfer",
                        description: "Creates a payment record without card capture.",
                      },
                      {
                        id: "CashOnDelivery",
                        label: "Cash on Delivery",
                        description: "Defers collection until delivery.",
                      },
                    ].map((method) => (
                      <label
                        key={method.id}
                        className={`block cursor-pointer border p-4 transition-colors ${
                          paymentMethod === method.id
                            ? "border-foreground bg-secondary/50"
                            : "border-border"
                        }`}
                      >
                        <input
                          type="radio"
                          name="paymentMethod"
                          value={method.id}
                          checked={paymentMethod === method.id}
                          onChange={() => setPaymentMethod(method.id as PaymentMethod)}
                          className="sr-only"
                        />
                        <p className="font-medium">{method.label}</p>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {method.description}
                        </p>
                      </label>
                    ))}
                  </div>

                  {selectedShipping ? (
                    <div className="mt-10 border border-border p-5 text-sm text-muted-foreground">
                      <p className="font-medium text-foreground">Shipping to</p>
                      <p>{selectedShipping.contactName}</p>
                      <p>
                        {selectedShipping.line1}, {selectedShipping.district}, {selectedShipping.city}
                      </p>
                    </div>
                  ) : null}

                  {selectedBilling ? (
                    <div className="mt-4 border border-border p-5 text-sm text-muted-foreground">
                      <p className="font-medium text-foreground">Billing to</p>
                      <p>{selectedBilling.contactName}</p>
                      <p>
                        {selectedBilling.line1}, {selectedBilling.district}, {selectedBilling.city}
                      </p>
                    </div>
                  ) : null}

                  {selectedCarrier ? (
                    <div className="mt-4 border border-border p-5 text-sm text-muted-foreground">
                      <p className="font-medium text-foreground">Shipping carrier</p>
                      <p>{selectedCarrier.name}</p>
                      <p>{selectedCarrier.serviceName || selectedCarrier.serviceCode || selectedCarrier.code}</p>
                    </div>
                  ) : null}

                  <div className="mt-12 flex items-center justify-between gap-4">
                    <Button
                      type="button"
                      variant="outline"
                      className="h-14 px-10 text-[11px] uppercase tracking-[0.2em] border-border hover:border-foreground hover:bg-transparent"
                      onClick={() => setCurrentStep("addresses")}
                    >
                      Back
                    </Button>
                    <Button
                      type="button"
                      className="h-14 px-12 text-[11px] uppercase tracking-[0.2em]"
                      disabled={isSubmitting}
                      onClick={() => void handlePlaceOrder()}
                    >
                      {isSubmitting ? "Submitting..." : `Place Order ${formatMoney(total, currencyCode)}`}
                    </Button>
                  </div>
                </div>
              )}

              {error ? <p className="mt-6 text-sm text-destructive">{error}</p> : null}
            </>
          )}
        </div>

        <div className="lg:col-span-1">
          <div className="sticky top-32 border border-border bg-card p-10">
            <h2 className="text-[11px] font-normal uppercase tracking-[0.25em] text-foreground">
              Order Summary
            </h2>

            <div className="mt-8 max-h-72 space-y-6 overflow-y-auto">
              {cart.map((item) => (
                <div
                  key={`${item.productId}-${item.variantId ?? "base"}`}
                  className="flex gap-4"
                >
                  <div className="relative h-24 w-20 shrink-0 overflow-hidden bg-secondary">
                    <Image
                      src={item.imageUrl || "/placeholder.jpg"}
                      alt={item.name}
                      fill
                      className="object-cover"
                    />
                  </div>
                  <div className="flex-1">
                    <p className="font-serif text-sm text-foreground line-clamp-1">
                      {item.name}
                    </p>
                    <p className="mt-1 text-[10px] uppercase tracking-wider text-muted-foreground">
                      Qty: {item.quantity}
                    </p>
                    {item.variantName ? (
                      <p className="mt-1 text-[10px] uppercase tracking-wider text-muted-foreground">
                        {item.variantName}
                      </p>
                    ) : null}
                    <p className="mt-2 text-sm tracking-wide text-foreground">
                      {formatMoney(item.priceAmount * item.quantity, item.currencyCode)}
                    </p>
                  </div>
                </div>
              ))}
            </div>

            <div className="mt-10 space-y-4 border-t border-border pt-8">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Subtotal</span>
                <span className="tracking-wide text-foreground">{formatMoney(subtotal, currencyCode)}</span>
              </div>
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Shipping</span>
                <span className="tracking-wide text-foreground">
                  {selectedCarrier ? selectedCarrier.name : "Choose carrier"}
                </span>
              </div>
              <div className="h-px w-full bg-border" />
              <div className="flex items-center justify-between pt-2">
                <span className="text-sm text-foreground">Estimated Total</span>
                <span className="text-xl tracking-wide text-foreground">
                  {formatMoney(total, currencyCode)}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
