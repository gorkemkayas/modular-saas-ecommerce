"use client"

import { useState, useTransition } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  getMyProfile,
  updateMyPreferences,
  updateMyProfile,
} from "@/lib/api/account"
import { getApiErrorMessage } from "@/lib/api/error-message"
import type { CustomerDto } from "@/lib/api/types"
import {
  validateOptionalText,
  validatePhoneNumber,
  validateRequiredText,
} from "@/lib/customer-validation"

interface PreferencesFormProps {
  customer: CustomerDto
}

export function PreferencesForm({ customer }: PreferencesFormProps) {
  const [profile, setProfile] = useState({
    firstName: customer.firstName,
    lastName: customer.lastName,
    phoneNumber: customer.phoneNumber ?? "",
  })
  const [preferences, setPreferences] = useState({
    preferredLanguage: customer.preferences.preferredLanguage ?? "",
    preferredCurrency: customer.preferences.preferredCurrency ?? "",
  })
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isPending, startTransition] = useTransition()

  function syncFromCustomerProfile(nextCustomer: CustomerDto) {
    setProfile({
      firstName: nextCustomer.firstName,
      lastName: nextCustomer.lastName,
      phoneNumber: nextCustomer.phoneNumber ?? "",
    })
    setPreferences({
      preferredLanguage: nextCustomer.preferences.preferredLanguage ?? "",
      preferredCurrency: nextCustomer.preferences.preferredCurrency ?? "",
    })
  }

  function validateForm(): string | null {
    return (
      validateRequiredText(profile.firstName, "First name", 100) ??
      validateRequiredText(profile.lastName, "Last name", 100) ??
      validatePhoneNumber(profile.phoneNumber, { required: false }) ??
      validateOptionalText(
        preferences.preferredLanguage,
        "Preferred language",
        10,
      ) ??
      validateOptionalText(
        preferences.preferredCurrency,
        "Preferred currency",
        3,
      )
    )
  }

  function handleSave() {
    setMessage(null)
    setError(null)

    const validationError = validateForm()
    if (validationError) {
      setError(validationError)
      return
    }

    startTransition(async () => {
      try {
        await updateMyProfile({
          firstName: profile.firstName.trim(),
          lastName: profile.lastName.trim(),
          phoneNumber: profile.phoneNumber.trim() || null,
        })
        await updateMyPreferences({
          preferredLanguage: preferences.preferredLanguage.trim() || null,
          preferredCurrency: preferences.preferredCurrency.trim() || null,
        })

        const refreshedProfile = await getMyProfile()
        syncFromCustomerProfile(refreshedProfile)

        setMessage("Preferences updated successfully.")
      } catch (saveError) {
        try {
          const refreshedProfile = await getMyProfile()
          syncFromCustomerProfile(refreshedProfile)
        } catch {
          // Keep the optimistic local state when the profile refresh also fails.
        }

        setError(
          getApiErrorMessage(
            saveError,
            "We could not save your preferences right now. Please try again.",
          ),
        )
      }
    })
  }

  return (
    <div className="space-y-8">
      <section className="border border-border">
        <div className="p-6 border-b border-border">
          <h3 className="text-sm font-medium tracking-wide">Profile Details</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Keep your basic account information up to date.
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 p-6">
          <div>
            <label className="block text-xs tracking-[0.2em] uppercase mb-3">
              First Name
            </label>
            <Input
              value={profile.firstName}
              onChange={(event) =>
                setProfile((current) => ({
                  ...current,
                  firstName: event.target.value,
                }))
              }
              className="h-12 bg-secondary border-0"
            />
          </div>

          <div>
            <label className="block text-xs tracking-[0.2em] uppercase mb-3">
              Last Name
            </label>
            <Input
              value={profile.lastName}
              onChange={(event) =>
                setProfile((current) => ({
                  ...current,
                  lastName: event.target.value,
                }))
              }
              className="h-12 bg-secondary border-0"
            />
          </div>

          <div className="sm:col-span-2">
            <label className="block text-xs tracking-[0.2em] uppercase mb-3">
              Phone Number
            </label>
            <Input
              value={profile.phoneNumber}
              onChange={(event) =>
                setProfile((current) => ({
                  ...current,
                  phoneNumber: event.target.value,
                }))
              }
              className="h-12 bg-secondary border-0"
            />
          </div>
        </div>
      </section>

      <section className="border border-border">
        <div className="p-6 border-b border-border">
          <h3 className="text-sm font-medium tracking-wide">Regional Settings</h3>
          <p className="text-sm text-muted-foreground mt-1">
            These values map directly to the customer preferences stored in the backend.
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 p-6">
          <div>
            <label className="block text-xs tracking-[0.2em] uppercase mb-3">
              Preferred Language
            </label>
            <Input
              placeholder="tr-TR"
              value={preferences.preferredLanguage}
              onChange={(event) =>
                setPreferences((current) => ({
                  ...current,
                  preferredLanguage: event.target.value,
                }))
              }
              className="h-12 bg-secondary border-0"
            />
          </div>

          <div>
            <label className="block text-xs tracking-[0.2em] uppercase mb-3">
              Preferred Currency
            </label>
            <Input
              placeholder="TRY"
              value={preferences.preferredCurrency}
              onChange={(event) =>
                setPreferences((current) => ({
                  ...current,
                  preferredCurrency: event.target.value.toUpperCase(),
                }))
              }
              className="h-12 bg-secondary border-0"
            />
          </div>
        </div>
      </section>

      {message ? (
        <div className="border border-border bg-secondary/30 px-6 py-4 text-sm">
          {message}
        </div>
      ) : null}

      {error ? (
        <div className="border border-destructive/30 bg-destructive/5 px-6 py-4 text-sm text-destructive">
          {error}
        </div>
      ) : null}

      <div className="flex justify-end">
        <Button
          onClick={handleSave}
          disabled={isPending}
          className="h-12 px-8 bg-primary text-primary-foreground text-sm tracking-[0.2em] uppercase"
        >
          {isPending ? "Saving..." : "Save Changes"}
        </Button>
      </div>
    </div>
  )
}
