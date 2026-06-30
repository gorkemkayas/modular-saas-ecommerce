"use client"

import { ChevronDown } from "lucide-react"
import { useRouter } from "next/navigation"
import { useEffect, useState, useTransition } from "react"

import {
  createAttributeDefinition,
  createBrand,
  createCategory,
  createNotificationTemplate,
  createPriceList,
  uploadCategoryImageFile,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import {
  formatSubscriptionLimit,
  getSubscriptionQuotaLimit,
  subscriptionQuotaKeys,
  type TenantSubscriptionDto,
} from "@/lib/api/subscription"

type CategoryOption = {
  id: string
  name: string
  depth: number
}

const priceListCurrencies = [
  { code: "TRY", flag: "🇹🇷", label: "Turkish Lira" },
  { code: "USD", flag: "🇺🇸", label: "US Dollar" },
  { code: "EUR", flag: "🇪🇺", label: "Euro" },
  { code: "GBP", flag: "🇬🇧", label: "British Pound" },
  { code: "CHF", flag: "🇨🇭", label: "Swiss Franc" },
  { code: "SEK", flag: "🇸🇪", label: "Swedish Krona" },
  { code: "NOK", flag: "🇳🇴", label: "Norwegian Krone" },
  { code: "DKK", flag: "🇩🇰", label: "Danish Krone" },
  { code: "PLN", flag: "🇵🇱", label: "Polish Zloty" },
  { code: "CZK", flag: "🇨🇿", label: "Czech Koruna" },
  { code: "HUF", flag: "🇭🇺", label: "Hungarian Forint" },
  { code: "RON", flag: "🇷🇴", label: "Romanian Leu" },
  { code: "AED", flag: "🇦🇪", label: "UAE Dirham" },
  { code: "SAR", flag: "🇸🇦", label: "Saudi Riyal" },
  { code: "QAR", flag: "🇶🇦", label: "Qatari Riyal" },
  { code: "KWD", flag: "🇰🇼", label: "Kuwaiti Dinar" },
  { code: "EGP", flag: "🇪🇬", label: "Egyptian Pound" },
  { code: "JPY", flag: "🇯🇵", label: "Japanese Yen" },
  { code: "CNY", flag: "🇨🇳", label: "Chinese Yuan" },
  { code: "KRW", flag: "🇰🇷", label: "South Korean Won" },
  { code: "INR", flag: "🇮🇳", label: "Indian Rupee" },
  { code: "AUD", flag: "🇦🇺", label: "Australian Dollar" },
  { code: "CAD", flag: "🇨🇦", label: "Canadian Dollar" },
  { code: "NZD", flag: "🇳🇿", label: "New Zealand Dollar" },
  { code: "SGD", flag: "🇸🇬", label: "Singapore Dollar" },
  { code: "HKD", flag: "🇭🇰", label: "Hong Kong Dollar" },
  { code: "THB", flag: "🇹🇭", label: "Thai Baht" },
  { code: "IDR", flag: "🇮🇩", label: "Indonesian Rupiah" },
  { code: "MYR", flag: "🇲🇾", label: "Malaysian Ringgit" },
  { code: "PHP", flag: "🇵🇭", label: "Philippine Peso" },
  { code: "ZAR", flag: "🇿🇦", label: "South African Rand" },
  { code: "BRL", flag: "🇧🇷", label: "Brazilian Real" },
  { code: "MXN", flag: "🇲🇽", label: "Mexican Peso" },
]

function FormError({ error }: { error: string | null }) {
  if (!error) {
    return null
  }

  return (
    <div className="border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive">
      {error}
    </div>
  )
}

export function AdminBrandCreateForm() {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [name, setName] = useState("")
  const [slug, setSlug] = useState("")
  const [description, setDescription] = useState("")
  const [error, setError] = useState<string | null>(null)

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setError(null)

    startTransition(async () => {
      try {
        await createBrand({
          name: name.trim(),
          slug: slug.trim(),
          description: description.trim() || null,
        })
        setName("")
        setSlug("")
        setDescription("")
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Brand creation failed."))
      }
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4 border border-border p-4">
      <h2 className="text-sm uppercase tracking-[0.2em] text-muted-foreground">Create Brand</h2>
      <div className="grid gap-4 md:grid-cols-3">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Name" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <input value={slug} onChange={(e) => setSlug(e.target.value)} placeholder="slug" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Description" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
      </div>
      <button disabled={isPending} className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60">
        {isPending ? "Creating..." : "Create Brand"}
      </button>
      <FormError error={error} />
    </form>
  )
}

export function AdminCategoryCreateForm({
  categories,
  subscription,
  currentCategoryCount,
}: {
  categories: CategoryOption[]
  subscription?: TenantSubscriptionDto | null
  currentCategoryCount?: number
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [name, setName] = useState("")
  const [slug, setSlug] = useState("")
  const [description, setDescription] = useState("")
  const [parentCategoryId, setParentCategoryId] = useState("")
  const [sortOrder, setSortOrder] = useState("0")
  const [imageUrl, setImageUrl] = useState<string | null>(null)
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null)
  const [imageFileName, setImageFileName] = useState<string | null>(null)
  const [isImageUploading, setIsImageUploading] = useState(false)
  const [imageUploadProgress, setImageUploadProgress] = useState(0)
  const [error, setError] = useState<string | null>(null)
  const categoryLimit = getSubscriptionQuotaLimit(
    subscription,
    subscriptionQuotaKeys.catalogCategories,
  )
  const isCategoryLimitReached =
    typeof categoryLimit === "number" &&
    typeof currentCategoryCount === "number" &&
    currentCategoryCount >= categoryLimit

  useEffect(() => {
    return () => {
      if (imagePreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(imagePreviewUrl)
      }
    }
  }, [imagePreviewUrl])

  async function handleImageChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]

    if (!file) {
      return
    }

    if (!file.type.startsWith("image/")) {
      setError("Only image files are supported for category images.")
      event.target.value = ""
      return
    }

    setError(null)
    setIsImageUploading(true)
    setImageUploadProgress(0)

    const previewUrl = URL.createObjectURL(file)
    if (imagePreviewUrl?.startsWith("blob:")) {
      URL.revokeObjectURL(imagePreviewUrl)
    }

    setImagePreviewUrl(previewUrl)
    setImageFileName(file.name)

    try {
      const uploadedFile = await uploadCategoryImageFile(file, setImageUploadProgress)
      setImageUrl(uploadedFile.url)
      setImageFileName(uploadedFile.originalFileName)
    } catch (uploadError) {
      setImageUrl(null)
      setError(getApiErrorMessage(uploadError, "Category image upload failed."))
    } finally {
      setIsImageUploading(false)
      event.target.value = ""
    }
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setError(null)

    if (isCategoryLimitReached) {
      setError(
        `Category limit reached for your current plan (${currentCategoryCount}/${categoryLimit}).`,
      )
      return
    }

    startTransition(async () => {
      try {
        await createCategory({
          name: name.trim(),
          slug: slug.trim(),
          description: description.trim() || null,
          imageUrl,
          parentCategoryId: parentCategoryId || null,
          sortOrder: Number(sortOrder || "0"),
        })
        setName("")
        setSlug("")
        setDescription("")
        setParentCategoryId("")
        setSortOrder("0")
        setImageUrl(null)
        if (imagePreviewUrl?.startsWith("blob:")) {
          URL.revokeObjectURL(imagePreviewUrl)
        }
        setImagePreviewUrl(null)
        setImageFileName(null)
        setImageUploadProgress(0)
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Category creation failed."))
      }
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4 border border-border p-4">
      <h2 className="text-sm uppercase tracking-[0.2em] text-muted-foreground">Create Category</h2>
      {typeof categoryLimit === "number" ? (
        <p className="text-xs text-muted-foreground">
          Current plan allows {formatSubscriptionLimit(categoryLimit)} categories.
          This store currently has {currentCategoryCount ?? 0}.
        </p>
      ) : null}
      <div className="grid gap-4 md:grid-cols-2">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Name" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <input value={slug} onChange={(e) => setSlug(e.target.value)} placeholder="slug" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Description" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <input type="number" value={sortOrder} onChange={(e) => setSortOrder(e.target.value)} placeholder="Sort Order" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <select value={parentCategoryId} onChange={(e) => setParentCategoryId(e.target.value)} className="md:col-span-2 bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground">
          <option value="">Top level</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {" ".repeat(category.depth * 2)}
              {category.name}
            </option>
          ))}
        </select>
        <label className="md:col-span-2 space-y-3 border border-border p-4">
          <span className="block text-[11px] uppercase tracking-[0.2em] text-muted-foreground">
            Category Image
          </span>
          <input
            type="file"
            accept="image/*"
            onChange={handleImageChange}
            className="block w-full text-sm file:mr-4 file:border-0 file:bg-secondary file:px-3 file:py-2 file:text-sm"
          />
          <p className="text-xs text-muted-foreground">
            Upload an image from your computer. The file is stored first, then its URL is saved on the category.
          </p>
          {imagePreviewUrl ? (
            <div className="flex items-center gap-4 border border-border bg-secondary/20 p-3">
              <img
                src={imagePreviewUrl}
                alt={imageFileName ?? "Category preview"}
                className="h-20 w-16 object-cover"
              />
              <div className="min-w-0 space-y-1 text-sm">
                <p className="truncate">{imageFileName ?? "Selected image"}</p>
                <p className="text-xs text-muted-foreground">
                  {isImageUploading
                    ? `Uploading ${imageUploadProgress}%`
                    : imageUrl
                      ? "Ready to save"
                      : "Waiting to upload"}
                </p>
              </div>
            </div>
          ) : null}
        </label>
      </div>
      <button disabled={isPending || isImageUploading || isCategoryLimitReached} className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60">
        {isPending
          ? "Creating..."
          : isCategoryLimitReached
            ? "Category Limit Reached"
            : "Create Category"}
      </button>
      <FormError error={error} />
    </form>
  )
}

export function AdminAttributeCreateForm() {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [name, setName] = useState("")
  const [code, setCode] = useState("")
  const [dataType, setDataType] = useState("String")
  const [isRequired, setIsRequired] = useState(false)
  const [isFilterable, setIsFilterable] = useState(false)
  const [isVariantDefining, setIsVariantDefining] = useState(false)
  const [error, setError] = useState<string | null>(null)

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setError(null)

    startTransition(async () => {
      try {
        await createAttributeDefinition({
          name: name.trim(),
          code: code.trim(),
          dataType,
          isRequired,
          isFilterable,
          isVariantDefining,
        })
        setName("")
        setCode("")
        setDataType("String")
        setIsRequired(false)
        setIsFilterable(false)
        setIsVariantDefining(false)
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Attribute creation failed."))
      }
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4 border border-border p-4">
      <h2 className="text-sm uppercase tracking-[0.2em] text-muted-foreground">Create Attribute</h2>
      <div className="grid gap-4 md:grid-cols-3">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Name" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <input value={code} onChange={(e) => setCode(e.target.value)} placeholder="Code" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <select value={dataType} onChange={(e) => setDataType(e.target.value)} className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground">
          <option value="String">String</option>
          <option value="Integer">Integer</option>
          <option value="Decimal">Decimal</option>
          <option value="Boolean">Boolean</option>
        </select>
      </div>
      <div className="flex flex-wrap gap-4 text-sm">
        <label className="flex items-center gap-2"><input type="checkbox" checked={isRequired} onChange={(e) => setIsRequired(e.target.checked)} />Required</label>
        <label className="flex items-center gap-2"><input type="checkbox" checked={isFilterable} onChange={(e) => setIsFilterable(e.target.checked)} />Filterable</label>
        <label className="flex items-center gap-2"><input type="checkbox" checked={isVariantDefining} onChange={(e) => setIsVariantDefining(e.target.checked)} />Variant defining</label>
      </div>
      <button disabled={isPending} className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60">
        {isPending ? "Creating..." : "Create Attribute"}
      </button>
      <FormError error={error} />
    </form>
  )
}

export function AdminPriceListCreateForm({
  subscription,
  currentPriceListCount,
}: {
  subscription?: TenantSubscriptionDto | null
  currentPriceListCount?: number
}) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [name, setName] = useState("")
  const [currencyCode, setCurrencyCode] = useState("TRY")
  const [isCurrencyPickerOpen, setIsCurrencyPickerOpen] = useState(false)
  const [priority, setPriority] = useState("")
  const [isDefault, setIsDefault] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const priceListLimit = getSubscriptionQuotaLimit(
    subscription,
    subscriptionQuotaKeys.pricingPriceLists,
  )
  const isPriceListLimitReached =
    typeof priceListLimit === "number" &&
    typeof currentPriceListCount === "number" &&
    currentPriceListCount >= priceListLimit
  const selectedCurrency =
    priceListCurrencies.find((currency) => currency.code === currencyCode) ??
    priceListCurrencies[0]

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setError(null)

    if (isPriceListLimitReached) {
      setError(
        `Price list limit reached for your current plan (${currentPriceListCount}/${priceListLimit}).`,
      )
      return
    }

    startTransition(async () => {
      try {
        await createPriceList({
          name: name.trim(),
          currencyCode: currencyCode.trim().toUpperCase(),
          priority: Number(priority || "0"),
          isDefault,
        })
        setName("")
        setCurrencyCode("TRY")
        setIsCurrencyPickerOpen(false)
        setPriority("")
        setIsDefault(false)
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Price list creation failed."))
      }
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4 border border-border p-4">
      <h2 className="text-sm uppercase tracking-[0.2em] text-muted-foreground">Create Price List</h2>
      {typeof priceListLimit === "number" ? (
        <p className="text-xs text-muted-foreground">
          Current plan allows {formatSubscriptionLimit(priceListLimit)} price lists.
          This store currently has {currentPriceListCount ?? 0}.
        </p>
      ) : null}
      <input
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="Name"
        className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
      />

      <div className="space-y-2">
        <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
          Currency
        </p>
        <div className="border border-border">
          <button
            type="button"
            onClick={() => setIsCurrencyPickerOpen((current) => !current)}
            className="flex w-full items-center justify-between gap-4 px-4 py-3 text-left transition-colors hover:bg-secondary/50"
          >
            <span className="flex min-w-0 items-center gap-3">
              <span className="text-base leading-none">{selectedCurrency.flag}</span>
              <span className="min-w-0">
                <span className="block text-sm">{selectedCurrency.code}</span>
                <span className="block text-[11px] text-muted-foreground">
                  {selectedCurrency.label}
                </span>
              </span>
            </span>
            <span className="flex items-center gap-2 text-xs uppercase tracking-[0.2em] text-muted-foreground">
              Select
              <ChevronDown
                className={`h-4 w-4 transition-transform ${
                  isCurrencyPickerOpen ? "rotate-180" : ""
                }`}
                strokeWidth={1.5}
              />
            </span>
          </button>

          {isCurrencyPickerOpen ? (
            <div className="grid max-h-72 gap-2 overflow-y-auto border-t border-border p-3 sm:grid-cols-2 xl:grid-cols-3">
              {priceListCurrencies.map((currency) => (
                <button
                  key={currency.code}
                  type="button"
                  onClick={() => {
                    setCurrencyCode(currency.code)
                    setIsCurrencyPickerOpen(false)
                  }}
                  className={`flex items-center gap-3 border px-3 py-2.5 text-left transition-colors ${
                    currencyCode === currency.code
                      ? "border-foreground bg-secondary"
                      : "border-border hover:bg-secondary/60"
                  }`}
                >
                  <span className="text-base leading-none">{currency.flag}</span>
                  <span className="min-w-0">
                    <span className="block text-sm">{currency.code}</span>
                    <span className="block text-[11px] text-muted-foreground">
                      {currency.label}
                    </span>
                  </span>
                </button>
              ))}
            </div>
          ) : null}
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_minmax(220px,280px)]">
        <label className="border border-border bg-secondary/35 p-4">
          <span className="mb-2 block text-[11px] uppercase tracking-[0.2em] text-muted-foreground">
            Priority
          </span>
          <input
            type="number"
            value={priority}
            onChange={(e) => setPriority(e.target.value)}
            placeholder="Priority"
            className="w-full bg-transparent text-sm focus:outline-none"
          />
          <span className="mt-2 block text-[11px] text-muted-foreground">
            Higher value wins when lists overlap.
          </span>
        </label>

        <button
          type="button"
          onClick={() => setIsDefault((current) => !current)}
          className={`flex items-center justify-between border p-4 text-left transition-colors ${
            isDefault
              ? "border-foreground bg-secondary/50"
              : "border-border bg-secondary/20 hover:bg-secondary/40"
          }`}
        >
          <span>
            <span className="block text-[11px] uppercase tracking-[0.2em] text-muted-foreground">
              Default List
            </span>
            <span className="mt-2 block text-sm">
              {isDefault ? "Used as the store fallback list." : "Mark this list as the fallback."}
            </span>
          </span>

          <span
            className={`relative inline-flex h-6 w-11 items-center rounded-full px-1 transition-colors ${
              isDefault ? "bg-foreground" : "bg-border"
            }`}
          >
            <span
              className={`h-4 w-4 rounded-full bg-background transition-transform ${
                isDefault ? "translate-x-5" : "translate-x-0"
              }`}
            />
          </span>
        </button>
      </div>
      <button disabled={isPending || isPriceListLimitReached} className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60">
        {isPending
          ? "Creating..."
          : isPriceListLimitReached
            ? "Price List Limit Reached"
            : "Create Price List"}
      </button>
      <FormError error={error} />
    </form>
  )
}

export function AdminNotificationTemplateCreateForm() {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [name, setName] = useState("")
  const [trigger, setTrigger] = useState("OrderPlaced")
  const [channel, setChannel] = useState("Email")
  const [locale, setLocale] = useState("default")
  const [subjectTemplate, setSubjectTemplate] = useState("")
  const [bodyTemplate, setBodyTemplate] = useState("")
  const [error, setError] = useState<string | null>(null)

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    setError(null)

    startTransition(async () => {
      try {
        await createNotificationTemplate({
          trigger,
          channel,
          locale,
          name: name.trim(),
          subjectTemplate: subjectTemplate.trim(),
          bodyTemplate: bodyTemplate.trim(),
        })
        setName("")
        setSubjectTemplate("")
        setBodyTemplate("")
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Template creation failed."))
      }
    })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4 border border-border p-4">
      <h2 className="text-sm uppercase tracking-[0.2em] text-muted-foreground">Create Template</h2>
      <div className="grid gap-4 md:grid-cols-4">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Name" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
        <select value={trigger} onChange={(e) => setTrigger(e.target.value)} className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground">
          <option value="OrderPlaced">OrderPlaced</option>
          <option value="OrderCancelled">OrderCancelled</option>
          <option value="PaymentAuthorized">PaymentAuthorized</option>
          <option value="PaymentCaptured">PaymentCaptured</option>
          <option value="PaymentFailed">PaymentFailed</option>
          <option value="PaymentRefunded">PaymentRefunded</option>
          <option value="ShipmentCreated">ShipmentCreated</option>
          <option value="ShipmentShipped">ShipmentShipped</option>
          <option value="ShipmentDelivered">ShipmentDelivered</option>
          <option value="ShipmentDeliveryException">ShipmentDeliveryException</option>
        </select>
        <select value={channel} onChange={(e) => setChannel(e.target.value)} className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground">
          <option value="Email">Email</option>
          <option value="Sms">Sms</option>
          <option value="Push">Push</option>
        </select>
        <input value={locale} onChange={(e) => setLocale(e.target.value)} placeholder="Locale" className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
      </div>
      <input value={subjectTemplate} onChange={(e) => setSubjectTemplate(e.target.value)} placeholder="Subject template" className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
      <textarea rows={4} value={bodyTemplate} onChange={(e) => setBodyTemplate(e.target.value)} placeholder="Body template" className="w-full resize-none bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground" />
      <button disabled={isPending} className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60">
        {isPending ? "Creating..." : "Create Template"}
      </button>
      <FormError error={error} />
    </form>
  )
}
