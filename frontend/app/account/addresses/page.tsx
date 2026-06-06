"use client"

import { useCallback, useEffect, useState, type FormEvent } from "react"
import {
  CreditCard,
  Edit2,
  Loader2,
  MapPin,
  Plus,
  Trash2,
  Truck,
  X,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  addMyAddress,
  deleteMyAddress,
  getMyProfile,
  setDefaultBillingAddress,
  setDefaultShippingAddress,
  updateMyAddress,
} from "@/lib/api/account"
import { getApiErrorMessage } from "@/lib/api/error-message"
import type {
  AddressType,
  CreateAddressRequest,
  CustomerAddressDto,
  UpdateAddressRequest,
} from "@/lib/api/types"
import {
  normalizeOptionalInput,
  validateOptionalText,
  validatePhoneNumber,
  validateRequiredText,
} from "@/lib/customer-validation"

type FormState = {
  addressType: AddressType
  title: string
  contactName: string
  phoneNumber: string
  country: string
  city: string
  district: string
  line1: string
  line2: string
  postalCode: string
}

const emptyFormState: FormState = {
  addressType: "Home",
  title: "",
  contactName: "",
  phoneNumber: "",
  country: "",
  city: "",
  district: "",
  line1: "",
  line2: "",
  postalCode: "",
}

function buildAddressPayload(formData: FormState): UpdateAddressRequest {
  return {
    addressType: formData.addressType,
    title: formData.title.trim(),
    contactName: formData.contactName.trim(),
    phoneNumber: formData.phoneNumber.trim(),
    country: formData.country.trim(),
    city: formData.city.trim(),
    district: formData.district.trim(),
    line1: formData.line1.trim(),
    line2: normalizeOptionalInput(formData.line2),
    postalCode: normalizeOptionalInput(formData.postalCode),
  }
}

function validateForm(formData: FormState): string | null {
  return (
    validateRequiredText(formData.title, "Address title", 100) ??
    validateRequiredText(formData.contactName, "Contact name", 200) ??
    validatePhoneNumber(formData.phoneNumber, { required: true }) ??
    validateRequiredText(formData.country, "Country", 100) ??
    validateRequiredText(formData.city, "City", 100) ??
    validateRequiredText(formData.district, "District", 100) ??
    validateRequiredText(formData.line1, "Address line 1", 500) ??
    validateOptionalText(formData.line2, "Address line 2", 500) ??
    validateOptionalText(formData.postalCode, "Postal code", 20)
  )
}

export default function AddressesPage() {
  const [addresses, setAddresses] = useState<CustomerAddressDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [activeAction, setActiveAction] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [isAdding, setIsAdding] = useState(false)
  const [formData, setFormData] = useState<FormState>(emptyFormState)
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null)

  const fetchAddresses = useCallback(async () => {
    try {
      setIsLoading(true)
      setError(null)
      const profile = await getMyProfile()
      setAddresses(profile.addresses)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, "Failed to load addresses."))
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    void fetchAddresses()
  }, [fetchAddresses])

  useEffect(() => {
    if (!successMessage) {
      return
    }

    const timer = window.setTimeout(() => setSuccessMessage(null), 3000)
    return () => window.clearTimeout(timer)
  }, [successMessage])

  function resetForm() {
    setEditingId(null)
    setIsAdding(false)
    setFormData(emptyFormState)
  }

  function handleAdd() {
    resetForm()
    setIsAdding(true)
    setError(null)
  }

  function handleEdit(address: CustomerAddressDto) {
    setEditingId(address.id)
    setIsAdding(false)
    setError(null)
    setFormData({
      addressType: address.addressType,
      title: address.title,
      contactName: address.contactName,
      phoneNumber: address.phoneNumber,
      country: address.country,
      city: address.city,
      district: address.district,
      line1: address.line1,
      line2: address.line2 ?? "",
      postalCode: address.postalCode ?? "",
    })
  }

  function handleCancel() {
    resetForm()
    setError(null)
  }

  async function handleSave(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validationError = validateForm(formData)
    if (validationError) {
      setError(validationError)
      return
    }

    if (isAdding && addresses.length >= 20) {
      setError("You can save up to 20 addresses in your account.")
      return
    }

    try {
      setIsSaving(true)
      setError(null)

      const payload = buildAddressPayload(formData)

      if (isAdding) {
        const request: CreateAddressRequest = {
          ...payload,
          isDefaultShipping: addresses.length === 0,
          isDefaultBilling: addresses.length === 0,
        }
        await addMyAddress(request)
        setSuccessMessage("Address added successfully.")
      } else if (editingId) {
        await updateMyAddress(editingId, payload)
        setSuccessMessage("Address updated successfully.")
      }

      await fetchAddresses()
      handleCancel()
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, "Failed to save address."))
    } finally {
      setIsSaving(false)
    }
  }

  async function handleDelete(addressId: string) {
    try {
      setIsDeleting(true)
      setError(null)
      await deleteMyAddress(addressId)
      setSuccessMessage("Address deleted successfully.")
      await fetchAddresses()
      setDeleteConfirmId(null)
    } catch (deleteError) {
      setError(getApiErrorMessage(deleteError, "Failed to delete address."))
    } finally {
      setIsDeleting(false)
    }
  }

  async function handleSetDefaultShipping(addressId: string) {
    try {
      setActiveAction(`shipping:${addressId}`)
      setError(null)
      await setDefaultShippingAddress(addressId)
      setSuccessMessage("Default shipping address updated.")
      await fetchAddresses()
    } catch (actionError) {
      setError(
        getApiErrorMessage(
          actionError,
          "Failed to set default shipping address.",
        ),
      )
    } finally {
      setActiveAction(null)
    }
  }

  async function handleSetDefaultBilling(addressId: string) {
    try {
      setActiveAction(`billing:${addressId}`)
      setError(null)
      await setDefaultBillingAddress(addressId)
      setSuccessMessage("Default billing address updated.")
      await fetchAddresses()
    } catch (actionError) {
      setError(
        getApiErrorMessage(
          actionError,
          "Failed to set default billing address.",
        ),
      )
    } finally {
      setActiveAction(null)
    }
  }

  function renderAddressForm() {
    return (
      <form onSubmit={handleSave} className="border border-border p-6 space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-xs tracking-[0.3em] uppercase">
              {isAdding ? "New Address" : "Edit Address"}
            </h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Save an address for faster checkout and account management.
            </p>
          </div>
          <button
            type="button"
            onClick={handleCancel}
            className="text-muted-foreground transition-colors hover:text-foreground"
            disabled={isSaving}
          >
            <X className="h-5 w-5" strokeWidth={1} />
          </button>
        </div>

        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Address Type
            </label>
            <div className="flex flex-wrap gap-4">
              {(["Home", "Work", "Other"] as AddressType[]).map((type) => (
                <button
                  key={type}
                  type="button"
                  onClick={() => setFormData((current) => ({ ...current, addressType: type }))}
                  className={`border px-4 py-2 text-xs tracking-[0.15em] uppercase transition-colors ${
                    formData.addressType === type
                      ? "border-foreground bg-foreground text-background"
                      : "border-border hover:border-foreground"
                  }`}
                >
                  {type}
                </button>
              ))}
            </div>
          </div>

          <div className="sm:col-span-2">
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Address Title
            </label>
            <Input
              type="text"
              placeholder="Home, Office"
              value={formData.title}
              onChange={(event) =>
                setFormData((current) => ({ ...current, title: event.target.value }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div className="sm:col-span-2">
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Contact Name
            </label>
            <Input
              type="text"
              value={formData.contactName}
              onChange={(event) =>
                setFormData((current) => ({
                  ...current,
                  contactName: event.target.value,
                }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div className="sm:col-span-2">
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Phone Number
            </label>
            <Input
              type="tel"
              value={formData.phoneNumber}
              onChange={(event) =>
                setFormData((current) => ({
                  ...current,
                  phoneNumber: event.target.value,
                }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div className="sm:col-span-2">
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Address Line 1
            </label>
            <Input
              type="text"
              placeholder="Street address"
              value={formData.line1}
              onChange={(event) =>
                setFormData((current) => ({ ...current, line1: event.target.value }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div className="sm:col-span-2">
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Address Line 2
            </label>
            <Input
              type="text"
              placeholder="Apartment, suite, etc."
              value={formData.line2}
              onChange={(event) =>
                setFormData((current) => ({ ...current, line2: event.target.value }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div>
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              District
            </label>
            <Input
              type="text"
              value={formData.district}
              onChange={(event) =>
                setFormData((current) => ({ ...current, district: event.target.value }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div>
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              City
            </label>
            <Input
              type="text"
              value={formData.city}
              onChange={(event) =>
                setFormData((current) => ({ ...current, city: event.target.value }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div>
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Postal Code
            </label>
            <Input
              type="text"
              value={formData.postalCode}
              onChange={(event) =>
                setFormData((current) => ({
                  ...current,
                  postalCode: event.target.value,
                }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>

          <div>
            <label className="mb-3 block text-xs tracking-[0.2em] uppercase">
              Country
            </label>
            <Input
              type="text"
              value={formData.country}
              onChange={(event) =>
                setFormData((current) => ({ ...current, country: event.target.value }))
              }
              className="h-12 border-0 bg-secondary"
            />
          </div>
        </div>

        {error ? (
          <div className="border border-destructive/20 bg-destructive/10 p-4 text-sm text-destructive">
            {error}
          </div>
        ) : null}

        <div className="flex items-center justify-end gap-4 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            className="h-12 border-border px-8"
            disabled={isSaving}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            className="h-12 bg-primary px-8 text-sm uppercase tracking-[0.2em] text-primary-foreground"
            disabled={isSaving}
          >
            {isSaving ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Saving...
              </>
            ) : (
              "Save Address"
            )}
          </Button>
        </div>
      </form>
    )
  }

  function renderDeleteConfirmModal(addressId: string) {
    const address = addresses.find((item) => item.id === addressId)
    if (!address) return null

    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
        <div className="mx-4 w-full max-w-md border border-border bg-background p-8">
          <h3 className="mb-4 text-lg font-medium">Delete Address</h3>
          <p className="mb-6 text-muted-foreground">
            Are you sure you want to delete &quot;{address.title}&quot;? This action
            cannot be undone.
          </p>
          <div className="flex items-center justify-end gap-4">
            <Button
              variant="outline"
              onClick={() => setDeleteConfirmId(null)}
              disabled={isDeleting}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => void handleDelete(addressId)}
              disabled={isDeleting}
            >
              {isDeleting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Deleting...
                </>
              ) : (
                "Delete"
              )}
            </Button>
          </div>
        </div>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex items-start justify-between gap-6">
        <div>
          <h2 className="text-xs tracking-[0.3em] uppercase">Saved Addresses</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Manage the delivery and billing addresses attached to your customer
            profile.
          </p>
        </div>
        {!isAdding && !editingId ? (
          <button
            onClick={handleAdd}
            className="flex items-center gap-2 text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            <Plus className="h-4 w-4" strokeWidth={1} />
            Add New
          </button>
        ) : null}
      </div>

      {successMessage ? (
        <div className="border border-emerald-500/20 bg-emerald-500/10 p-4 text-sm text-emerald-700 dark:text-emerald-400">
          {successMessage}
        </div>
      ) : null}

      {error && !isAdding && !editingId ? (
        <div className="border border-destructive/20 bg-destructive/10 p-4 text-sm text-destructive">
          {error}
        </div>
      ) : null}

      {(isAdding || editingId) ? renderAddressForm() : null}

      {!isAdding && !editingId ? (
        <div className="space-y-4">
          {addresses.map((address) => {
            const shippingActionKey = `shipping:${address.id}`
            const billingActionKey = `billing:${address.id}`

            return (
              <section
                key={address.id}
                className={`border p-6 ${
                  address.isDefaultShipping || address.isDefaultBilling
                    ? "border-foreground"
                    : "border-border"
                }`}
              >
                <div className="flex flex-col gap-6 sm:flex-row sm:items-start sm:justify-between">
                  <div className="flex items-start gap-4">
                    <div className="flex h-10 w-10 items-center justify-center bg-secondary">
                      <MapPin className="h-5 w-5" strokeWidth={1} />
                    </div>
                    <div>
                      <div className="mb-3 flex flex-wrap items-center gap-2 text-xs uppercase">
                        <span className="bg-secondary px-3 py-1 tracking-[0.15em] text-muted-foreground">
                          {address.title}
                        </span>
                        <span className="bg-secondary px-3 py-1 tracking-[0.15em] text-muted-foreground">
                          {address.addressType}
                        </span>
                        {address.isDefaultShipping ? (
                          <span className="flex items-center gap-1 bg-foreground px-3 py-1 tracking-[0.15em] text-background">
                            <Truck className="h-3 w-3" strokeWidth={1.5} />
                            Default Shipping
                          </span>
                        ) : null}
                        {address.isDefaultBilling ? (
                          <span className="flex items-center gap-1 bg-foreground px-3 py-1 tracking-[0.15em] text-background">
                            <CreditCard className="h-3 w-3" strokeWidth={1.5} />
                            Default Billing
                          </span>
                        ) : null}
                      </div>

                      <p className="font-medium tracking-wide">{address.contactName}</p>
                      <div className="mt-2 space-y-1 text-sm text-muted-foreground">
                        <p>{address.line1}</p>
                        {address.line2 ? <p>{address.line2}</p> : null}
                        <p>
                          {address.district}, {address.city}
                          {address.postalCode ? ` ${address.postalCode}` : ""}
                        </p>
                        <p>{address.country}</p>
                        <p>{address.phoneNumber}</p>
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center gap-1 self-end sm:self-start">
                    {!address.isDefaultShipping ? (
                      <button
                        onClick={() => void handleSetDefaultShipping(address.id)}
                        className="p-2 text-muted-foreground transition-colors hover:text-foreground"
                        title="Set as default shipping"
                        disabled={activeAction !== null}
                      >
                        {activeAction === shippingActionKey ? (
                          <Loader2 className="h-4 w-4 animate-spin" />
                        ) : (
                          <Truck className="h-4 w-4" strokeWidth={1} />
                        )}
                      </button>
                    ) : null}
                    {!address.isDefaultBilling ? (
                      <button
                        onClick={() => void handleSetDefaultBilling(address.id)}
                        className="p-2 text-muted-foreground transition-colors hover:text-foreground"
                        title="Set as default billing"
                        disabled={activeAction !== null}
                      >
                        {activeAction === billingActionKey ? (
                          <Loader2 className="h-4 w-4 animate-spin" />
                        ) : (
                          <CreditCard className="h-4 w-4" strokeWidth={1} />
                        )}
                      </button>
                    ) : null}
                    <button
                      onClick={() => handleEdit(address)}
                      className="p-2 text-muted-foreground transition-colors hover:text-foreground"
                      title="Edit address"
                      disabled={activeAction !== null}
                    >
                      <Edit2 className="h-4 w-4" strokeWidth={1} />
                    </button>
                    <button
                      onClick={() => setDeleteConfirmId(address.id)}
                      className="p-2 text-muted-foreground transition-colors hover:text-destructive"
                      title="Delete address"
                      disabled={activeAction !== null}
                    >
                      <Trash2 className="h-4 w-4" strokeWidth={1} />
                    </button>
                  </div>
                </div>
              </section>
            )
          })}

          {!addresses.length ? (
            <div className="border border-border py-16 text-center">
              <MapPin
                className="mx-auto mb-4 h-12 w-12 text-muted-foreground"
                strokeWidth={1}
              />
              <p className="mb-4 text-muted-foreground">No addresses saved yet.</p>
              <Button onClick={handleAdd} variant="outline" className="h-12 px-8">
                Add Your First Address
              </Button>
            </div>
          ) : null}
        </div>
      ) : null}

      {deleteConfirmId ? renderDeleteConfirmModal(deleteConfirmId) : null}
    </div>
  )
}
