"use client"

import { useEffect, useMemo, useState, useTransition } from "react"
import { Eye, Globe, Image as ImageIcon, Store, Truck } from "lucide-react"
import { useRouter } from "next/navigation"

import type { StoreDto } from "@/lib/api/types"
import {
  changeStoreSlug,
  checkStoreSlugAvailability,
  publishStore,
  suggestStoreSlug,
  unpublishStore,
  updateStoreProfile,
  uploadStoreHeroMediaFile,
} from "@/lib/api/admin"
import { storefrontPath } from "@/lib/config"
import { getApiErrorMessage } from "@/lib/api/error-message"
import {
  hasSubscriptionFeature,
  subscriptionFeatureKeys,
  type TenantSubscriptionDto,
} from "@/lib/api/subscription"
import { AdminShippingCarriersManager } from "@/components/admin/admin-shipping-carriers-manager"

const sections = [
  { id: "general", label: "General", icon: Store },
  { id: "slug", label: "Slug & URL", icon: Globe },
  { id: "branding", label: "Branding", icon: ImageIcon },
  { id: "shipping", label: "Shipping", icon: Truck },
  { id: "publishing", label: "Publishing", icon: Eye },
] as const

type SectionId = (typeof sections)[number]["id"]

function SectionNotice({
  kind,
  message,
}: {
  kind: "error" | "success" | "info"
  message: string | null
}) {
  if (!message) {
    return null
  }

  const className =
    kind === "error"
      ? "border-destructive/30 bg-destructive/5 text-destructive"
      : kind === "success"
        ? "border-emerald-500/30 bg-emerald-500/5 text-emerald-700"
        : "border-border bg-secondary/40 text-muted-foreground"

  return <div className={`border px-4 py-3 text-sm ${className}`}>{message}</div>
}

export function AdminStoreSettingsManager({
  initialStore,
  initialSection = "general",
  subscription,
}: {
  initialStore: StoreDto
  initialSection?: SectionId
  subscription?: TenantSubscriptionDto | null
}) {
  const router = useRouter()
  const [activeSection, setActiveSection] = useState<SectionId>(initialSection)
  const [storeName, setStoreName] = useState(initialStore.name)
  const [description, setDescription] = useState(initialStore.description ?? "")
  const [logoUrl, setLogoUrl] = useState(initialStore.logoUrl ?? "")
  const [heroImageUrl, setHeroImageUrl] = useState(initialStore.heroImageUrl ?? "")
  const [heroMediaType, setHeroMediaType] = useState(initialStore.heroMediaType ?? "")
  const [heroEyebrowText, setHeroEyebrowText] = useState(
    initialStore.heroEyebrowText ?? "",
  )
  const [heroTitle, setHeroTitle] = useState(initialStore.heroTitle ?? "")
  const [heroAccentTitle, setHeroAccentTitle] = useState(
    initialStore.heroAccentTitle ?? "",
  )
  const [heroDescription, setHeroDescription] = useState(
    initialStore.heroDescription ?? "",
  )
  const [heroPrimaryButtonText, setHeroPrimaryButtonText] = useState(
    initialStore.heroPrimaryButtonText ?? "",
  )
  const [loginPageImageUrl, setLoginPageImageUrl] = useState(
    initialStore.loginPageImageUrl ?? "",
  )
  const [registerPageImageUrl, setRegisterPageImageUrl] = useState(
    initialStore.registerPageImageUrl ?? "",
  )
  const [currentSlug, setCurrentSlug] = useState(initialStore.slug)
  const [slugDraft, setSlugDraft] = useState(initialStore.slug)
  const [isPublished, setIsPublished] = useState(initialStore.isPublished)
  const [status, setStatus] = useState(initialStore.status)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [slugInfo, setSlugInfo] = useState<string | null>(null)
  const [heroMediaPreviewUrl, setHeroMediaPreviewUrl] = useState<string | null>(
    initialStore.heroImageUrl ?? null,
  )
  const [heroMediaFileName, setHeroMediaFileName] = useState<string | null>(null)
  const [isHeroMediaUploading, setIsHeroMediaUploading] = useState(false)
  const [heroMediaUploadProgress, setHeroMediaUploadProgress] = useState(0)
  const [loginPageImagePreviewUrl, setLoginPageImagePreviewUrl] = useState<string | null>(
    initialStore.loginPageImageUrl ?? null,
  )
  const [registerPageImagePreviewUrl, setRegisterPageImagePreviewUrl] = useState<string | null>(
    initialStore.registerPageImageUrl ?? null,
  )
  const [loginPageImageFileName, setLoginPageImageFileName] = useState<string | null>(null)
  const [registerPageImageFileName, setRegisterPageImageFileName] = useState<string | null>(null)
  const [isLoginPageImageUploading, setIsLoginPageImageUploading] = useState(false)
  const [isRegisterPageImageUploading, setIsRegisterPageImageUploading] = useState(false)
  const [loginPageImageUploadProgress, setLoginPageImageUploadProgress] = useState(0)
  const [registerPageImageUploadProgress, setRegisterPageImageUploadProgress] = useState(0)
  const [isProfilePending, startProfileTransition] = useTransition()
  const [isSlugPending, startSlugTransition] = useTransition()
  const [isPublishPending, startPublishTransition] = useTransition()
  const canUseVideoHero =
    !subscription ||
    hasSubscriptionFeature(subscription, subscriptionFeatureKeys.storefrontVideoHero)

  const storefrontPreviewPath = useMemo(
    () => storefrontPath(currentSlug),
    [currentSlug],
  )

  useEffect(() => {
    return () => {
      if (heroMediaPreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(heroMediaPreviewUrl)
      }

      if (loginPageImagePreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(loginPageImagePreviewUrl)
      }

      if (registerPageImagePreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(registerPageImagePreviewUrl)
      }
    }
  }, [heroMediaPreviewUrl, loginPageImagePreviewUrl, registerPageImagePreviewUrl])

  function resetMessages() {
    setError(null)
    setSuccessMessage(null)
    setSlugInfo(null)
  }

  function handleSaveProfile() {
    resetMessages()

    startProfileTransition(async () => {
      try {
        await updateStoreProfile({
          name: storeName.trim(),
          description: description.trim() || null,
          logoUrl: logoUrl.trim() || null,
          heroImageUrl: heroImageUrl.trim() || null,
          heroMediaType: heroMediaType.trim() || null,
          heroEyebrowText: heroEyebrowText.trim() || null,
          heroTitle: heroTitle.trim() || null,
          heroAccentTitle: heroAccentTitle.trim() || null,
          heroDescription: heroDescription.trim() || null,
          heroPrimaryButtonText: heroPrimaryButtonText.trim() || null,
          loginPageImageUrl: loginPageImageUrl.trim() || null,
          registerPageImageUrl: registerPageImageUrl.trim() || null,
        })

        setSuccessMessage("Store profile was updated.")
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Store profile update failed."))
      }
    })
  }

  async function handleHeroMediaChange(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]

    if (!file) {
      return
    }

    const isVideo = file.type.startsWith("video/")
    const isSupported = file.type.startsWith("image/") || isVideo

    if (!isSupported) {
      setError("Only image and video files are supported for the storefront hero.")
      event.target.value = ""
      return
    }

    if (isVideo && !canUseVideoHero) {
      setError("Storefront video hero is not available in your current plan.")
      event.target.value = ""
      return
    }

    resetMessages()
    setIsHeroMediaUploading(true)
    setHeroMediaUploadProgress(0)

    const previewUrl = URL.createObjectURL(file)
    if (heroMediaPreviewUrl?.startsWith("blob:")) {
      URL.revokeObjectURL(heroMediaPreviewUrl)
    }

    setHeroMediaPreviewUrl(previewUrl)
    setHeroMediaFileName(file.name)

    try {
      const uploadedFile = await uploadStoreHeroMediaFile(file, setHeroMediaUploadProgress)
      setHeroImageUrl(uploadedFile.url)
      setHeroMediaType(uploadedFile.mediaType)
      setHeroMediaFileName(uploadedFile.originalFileName)
      setHeroMediaPreviewUrl(uploadedFile.url)
    } catch (uploadError) {
      setHeroMediaPreviewUrl(initialStore.heroImageUrl ?? null)
      setHeroImageUrl(initialStore.heroImageUrl ?? "")
      setHeroMediaType(initialStore.heroMediaType ?? "")
      setError(
        getApiErrorMessage(
          uploadError,
          "Storefront hero media upload failed.",
        ),
      )
    } finally {
      setIsHeroMediaUploading(false)
      event.target.value = ""
    }
  }

  async function handleAuthPageImageChange(
    target: "login" | "register",
    event: React.ChangeEvent<HTMLInputElement>,
  ) {
    const file = event.target.files?.[0]

    if (!file) {
      return
    }

    if (!file.type.startsWith("image/")) {
      setError("Only image files are supported for auth page visuals.")
      event.target.value = ""
      return
    }

    resetMessages()

    const currentPreviewUrl =
      target === "login" ? loginPageImagePreviewUrl : registerPageImagePreviewUrl
    const currentImageUrl = target === "login" ? loginPageImageUrl : registerPageImageUrl

    if (target === "login") {
      setIsLoginPageImageUploading(true)
      setLoginPageImageUploadProgress(0)
    } else {
      setIsRegisterPageImageUploading(true)
      setRegisterPageImageUploadProgress(0)
    }

    const previewUrl = URL.createObjectURL(file)
    if (currentPreviewUrl?.startsWith("blob:")) {
      URL.revokeObjectURL(currentPreviewUrl)
    }

    if (target === "login") {
      setLoginPageImagePreviewUrl(previewUrl)
      setLoginPageImageFileName(file.name)
    } else {
      setRegisterPageImagePreviewUrl(previewUrl)
      setRegisterPageImageFileName(file.name)
    }

    try {
      const uploadedFile = await uploadStoreHeroMediaFile(file, (progress) => {
        if (target === "login") {
          setLoginPageImageUploadProgress(progress)
        } else {
          setRegisterPageImageUploadProgress(progress)
        }
      })

      if (target === "login") {
        setLoginPageImageUrl(uploadedFile.url)
        setLoginPageImageFileName(uploadedFile.originalFileName)
        setLoginPageImagePreviewUrl(uploadedFile.url)
      } else {
        setRegisterPageImageUrl(uploadedFile.url)
        setRegisterPageImageFileName(uploadedFile.originalFileName)
        setRegisterPageImagePreviewUrl(uploadedFile.url)
      }
    } catch (uploadError) {
      if (target === "login") {
        setLoginPageImagePreviewUrl(currentImageUrl || null)
        setLoginPageImageUrl(currentImageUrl)
      } else {
        setRegisterPageImagePreviewUrl(currentImageUrl || null)
        setRegisterPageImageUrl(currentImageUrl)
      }

      setError(
        getApiErrorMessage(
          uploadError,
          `${target === "login" ? "Login" : "Register"} auth image upload failed.`,
        ),
      )
    } finally {
      if (target === "login") {
        setIsLoginPageImageUploading(false)
      } else {
        setIsRegisterPageImageUploading(false)
      }

      event.target.value = ""
    }
  }

  function handleCheckSlugAvailability() {
    resetMessages()

    startSlugTransition(async () => {
      try {
        const result = await checkStoreSlugAvailability(slugDraft.trim())
        setSlugInfo(
          result.isAvailable
            ? `/${result.slug} is available.`
            : `/${result.slug} is already taken.`,
        )
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Slug availability could not be checked."))
      }
    })
  }

  function handleSuggestSlug() {
    resetMessages()

    startSlugTransition(async () => {
      try {
        const result = await suggestStoreSlug(slugDraft.trim())
        setSlugDraft(result.slug)
        setSlugInfo(`Suggested slug: /${result.slug}`)
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "A slug suggestion could not be generated."))
      }
    })
  }

  function handleSaveSlug() {
    resetMessages()

    startSlugTransition(async () => {
      try {
        await changeStoreSlug(slugDraft.trim())
        setCurrentSlug(slugDraft.trim())
        setSuccessMessage("Store slug was updated.")
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Store slug update failed."))
      }
    })
  }

  function handlePublish(nextPublishedState: boolean) {
    resetMessages()

    startPublishTransition(async () => {
      try {
        if (nextPublishedState) {
          await publishStore()
          setIsPublished(true)
          setSuccessMessage("Store is now published.")
        } else {
          await unpublishStore()
          setIsPublished(false)
          setSuccessMessage("Store was unpublished.")
        }

        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Store publish state could not be updated."))
      }
    })
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-light tracking-wide">Store Settings</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          This screen is bound directly to the current store backend: profile, slug, storefront hero, logo URL, and publish state.
        </p>
      </div>

      <SectionNotice kind="error" message={error} />
      <SectionNotice kind="success" message={successMessage} />
      <SectionNotice kind="info" message={slugInfo} />

      <div className="flex flex-col gap-6 lg:flex-row">
        <aside className="flex-shrink-0 lg:w-56">
          <nav className="space-y-1">
            {sections.map((section) => {
              const Icon = section.icon
              return (
                <button
                  key={section.id}
                  type="button"
                  onClick={() => {
                    resetMessages()
                    setActiveSection(section.id)
                  }}
                  className={`flex w-full items-center gap-3 px-4 py-3 text-sm transition-colors ${
                    activeSection === section.id
                      ? "bg-primary text-primary-foreground"
                      : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                  }`}
                >
                  <Icon className="h-4 w-4" strokeWidth={1.5} />
                  {section.label}
                </button>
              )
            })}
          </nav>
        </aside>

        <div className="flex-1 border border-border p-6">
          {activeSection === "general" ? (
            <div className="space-y-6">
              <h2 className="border-b border-border pb-4 text-lg font-light">General Store Data</h2>
              <div className="grid gap-6">
                <div>
                  <label className="mb-2 block text-sm">Store Name</label>
                  <input
                    type="text"
                    value={storeName}
                    onChange={(event) => setStoreName(event.target.value)}
                    className="w-full max-w-md bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                  />
                </div>
                <div>
                  <label className="mb-2 block text-sm">Description</label>
                  <textarea
                    rows={4}
                    value={description}
                    onChange={(event) => setDescription(event.target.value)}
                    className="w-full max-w-2xl resize-none bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                  />
                </div>
              </div>
            </div>
          ) : null}

          {activeSection === "slug" ? (
            <div className="space-y-6">
              <h2 className="border-b border-border pb-4 text-lg font-light">Slug & Public URL</h2>
              <div className="grid gap-6">
                <div>
                  <label className="mb-2 block text-sm">Store Slug</label>
                  <input
                    type="text"
                    value={slugDraft}
                    onChange={(event) => setSlugDraft(event.target.value)}
                    className="w-full max-w-md bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                  />
                  <p className="mt-2 text-xs text-muted-foreground">
                    Slug availability and suggestion are checked against the backend before you update it.
                  </p>
                </div>
                <div className="flex flex-wrap gap-3">
                  <button
                    type="button"
                    onClick={handleCheckSlugAvailability}
                    disabled={isSlugPending}
                    className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
                  >
                    Check Availability
                  </button>
                  <button
                    type="button"
                    onClick={handleSuggestSlug}
                    disabled={isSlugPending}
                    className="border border-border px-4 py-3 text-sm transition-colors hover:bg-secondary disabled:opacity-60"
                  >
                    Suggest Slug
                  </button>
                </div>
                <div className="border border-border bg-secondary/30 p-4 text-sm text-muted-foreground">
                  Public storefront URL preview:{" "}
                  <span className="text-foreground">{storefrontPreviewPath}</span>
                </div>
              </div>
            </div>
          ) : null}

          {activeSection === "branding" ? (
            <div className="space-y-6">
              <h2 className="border-b border-border pb-4 text-lg font-light">Branding</h2>
              <div className="grid gap-6">
                <div>
                  <label className="mb-2 block text-sm">Logo URL</label>
                  <input
                    type="url"
                    value={logoUrl}
                    onChange={(event) => setLogoUrl(event.target.value)}
                    className="w-full max-w-2xl bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                  />
                  <p className="mt-2 text-xs text-muted-foreground">
                    The current backend accepts a logo URL here. File upload is not part of the store API yet.
                  </p>
                </div>

                <div className="border border-border p-5">
                  <h3 className="text-sm font-medium tracking-wide">Storefront Hero</h3>
                  <p className="mt-2 text-xs text-muted-foreground">
                    Leave these blank to keep the current default storefront hero image and copy.
                  </p>

                  <div className="mt-5 grid gap-5">
                    <div>
                      <label className="mb-2 block text-sm">Hero Media</label>
                      <div className="grid gap-4">
                        <label className="flex cursor-pointer items-center justify-between border border-dashed border-border px-4 py-4 text-sm transition-colors hover:bg-secondary/40">
                          <div>
                            <p className="font-medium">
                              {canUseVideoHero ? "Upload image or video" : "Upload image"}
                            </p>
                            <p className="mt-1 text-xs text-muted-foreground">
                              {canUseVideoHero
                                ? "Stored through the same upload flow used for product media."
                                : "Video hero requires a higher subscription plan."}
                            </p>
                          </div>
                          <span className="border border-border px-3 py-2 text-xs uppercase tracking-[0.2em]">
                            Choose File
                          </span>
                          <input
                            type="file"
                            accept={canUseVideoHero ? "image/*,video/*" : "image/*"}
                            className="hidden"
                            onChange={handleHeroMediaChange}
                          />
                        </label>

                        {heroMediaPreviewUrl ? (
                          heroMediaType.toLowerCase() === "video" ? (
                            <video
                              src={heroMediaPreviewUrl}
                              controls
                              muted
                              playsInline
                              className="aspect-[16/9] w-full max-w-3xl border border-border object-cover"
                            />
                          ) : (
                            <img
                              src={heroMediaPreviewUrl}
                              alt="Storefront hero preview"
                              className="aspect-[16/9] w-full max-w-3xl border border-border object-cover"
                            />
                          )
                        ) : null}

                        <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                          <span>
                            {heroMediaFileName
                              ? heroMediaFileName
                              : heroImageUrl
                                ? "Uploaded hero media is ready."
                                : "No custom hero media uploaded."}
                          </span>
                          {heroMediaType ? (
                            <span className="border border-border px-2 py-1 uppercase tracking-[0.2em] text-[10px] text-foreground">
                              {heroMediaType}
                            </span>
                          ) : null}
                          {isHeroMediaUploading ? (
                            <span>Uploading {heroMediaUploadProgress}%</span>
                          ) : null}
                        </div>
                      </div>
                    </div>

                    <div className="grid gap-5 lg:grid-cols-2">
                      <div>
                        <label className="mb-2 block text-sm">Hero Eyebrow</label>
                        <input
                          type="text"
                          value={heroEyebrowText}
                          onChange={(event) => setHeroEyebrowText(event.target.value)}
                          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm">Primary Button Text</label>
                        <input
                          type="text"
                          value={heroPrimaryButtonText}
                          onChange={(event) => setHeroPrimaryButtonText(event.target.value)}
                          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                        />
                      </div>
                    </div>

                    <div className="grid gap-5 lg:grid-cols-2">
                      <div>
                        <label className="mb-2 block text-sm">Hero Title</label>
                        <input
                          type="text"
                          value={heroTitle}
                          onChange={(event) => setHeroTitle(event.target.value)}
                          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm">Hero Accent Title</label>
                        <input
                          type="text"
                          value={heroAccentTitle}
                          onChange={(event) => setHeroAccentTitle(event.target.value)}
                          className="w-full bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                        />
                      </div>
                    </div>

                    <div>
                      <label className="mb-2 block text-sm">Hero Description</label>
                      <textarea
                        rows={4}
                        value={heroDescription}
                        onChange={(event) => setHeroDescription(event.target.value)}
                        className="w-full max-w-3xl resize-none bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
                      />
                    </div>
                  </div>
                </div>

                <div className="border border-border p-5">
                  <h3 className="text-sm font-medium tracking-wide">Auth Page Visuals</h3>
                  <p className="mt-2 text-xs text-muted-foreground">
                    Use separate store-specific visuals for customer sign in and registration pages.
                  </p>

                  <div className="mt-5 grid gap-6 xl:grid-cols-2">
                    <div className="space-y-4">
                      <div>
                        <label className="mb-2 block text-sm">Login Page Image</label>
                        <label className="flex cursor-pointer items-center justify-between border border-dashed border-border px-4 py-4 text-sm transition-colors hover:bg-secondary/40">
                          <div>
                            <p className="font-medium">Upload login visual</p>
                            <p className="mt-1 text-xs text-muted-foreground">
                              Shown on the customer sign in page for this store.
                            </p>
                          </div>
                          <span className="border border-border px-3 py-2 text-xs uppercase tracking-[0.2em]">
                            Choose File
                          </span>
                          <input
                            type="file"
                            accept="image/*"
                            className="hidden"
                            onChange={(event) => handleAuthPageImageChange("login", event)}
                          />
                        </label>
                      </div>

                      {loginPageImagePreviewUrl ? (
                        <img
                          src={loginPageImagePreviewUrl}
                          alt="Login page visual preview"
                          className="aspect-[3/4] w-full max-w-sm border border-border object-cover"
                        />
                      ) : null}

                      <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                        <span>
                          {loginPageImageFileName
                            ? loginPageImageFileName
                            : loginPageImageUrl
                              ? "Custom login image is ready."
                              : "No custom login image uploaded."}
                        </span>
                        {isLoginPageImageUploading ? (
                          <span>Uploading {loginPageImageUploadProgress}%</span>
                        ) : null}
                      </div>
                    </div>

                    <div className="space-y-4">
                      <div>
                        <label className="mb-2 block text-sm">Register Page Image</label>
                        <label className="flex cursor-pointer items-center justify-between border border-dashed border-border px-4 py-4 text-sm transition-colors hover:bg-secondary/40">
                          <div>
                            <p className="font-medium">Upload register visual</p>
                            <p className="mt-1 text-xs text-muted-foreground">
                              Shown on the customer registration page for this store.
                            </p>
                          </div>
                          <span className="border border-border px-3 py-2 text-xs uppercase tracking-[0.2em]">
                            Choose File
                          </span>
                          <input
                            type="file"
                            accept="image/*"
                            className="hidden"
                            onChange={(event) => handleAuthPageImageChange("register", event)}
                          />
                        </label>
                      </div>

                      {registerPageImagePreviewUrl ? (
                        <img
                          src={registerPageImagePreviewUrl}
                          alt="Register page visual preview"
                          className="aspect-[3/4] w-full max-w-sm border border-border object-cover"
                        />
                      ) : null}

                      <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                        <span>
                          {registerPageImageFileName
                            ? registerPageImageFileName
                            : registerPageImageUrl
                              ? "Custom register image is ready."
                              : "No custom register image uploaded."}
                        </span>
                        {isRegisterPageImageUploading ? (
                          <span>Uploading {registerPageImageUploadProgress}%</span>
                        ) : null}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ) : null}

          {activeSection === "publishing" ? (
            <div className="space-y-6">
              <h2 className="border-b border-border pb-4 text-lg font-light">Publishing State</h2>
              <div className="space-y-4">
                <div className="border border-border p-4">
                  <p className="text-sm font-medium">Current Status</p>
                  <div className="mt-2 space-y-1 text-sm text-muted-foreground">
                    <p>
                      Store status: <span className="text-foreground">{status}</span>
                    </p>
                    <p>
                      Publish state:{" "}
                      <span className="text-foreground">
                        {isPublished ? "Published" : "Unpublished"}
                      </span>
                    </p>
                  </div>
                </div>
                <div className="flex gap-4">
                  <button
                    type="button"
                    onClick={() => handlePublish(true)}
                    disabled={isPublishPending || isPublished || status !== "Active"}
                    className="bg-primary px-6 py-3 text-sm tracking-wide text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
                  >
                    Publish Store
                  </button>
                  <button
                    type="button"
                    onClick={() => handlePublish(false)}
                    disabled={isPublishPending || !isPublished}
                    className="border border-border px-6 py-3 text-sm tracking-wide transition-colors hover:bg-secondary disabled:opacity-60"
                  >
                    Unpublish Store
                  </button>
                </div>
              </div>
            </div>
          ) : null}

          {activeSection === "shipping" ? (
            <AdminShippingCarriersManager subscription={subscription} />
          ) : null}

          {activeSection === "general" || activeSection === "branding" ? (
            <div className="mt-6 flex justify-end border-t border-border pt-6">
              <button
                type="button"
                onClick={handleSaveProfile}
                disabled={isProfilePending}
                className="bg-primary px-8 py-3 text-sm tracking-wide text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
              >
                {isProfilePending ? "Saving..." : "Save Changes"}
              </button>
            </div>
          ) : null}

          {activeSection === "slug" ? (
            <div className="mt-6 flex justify-end border-t border-border pt-6">
              <button
                type="button"
                onClick={handleSaveSlug}
                disabled={isSlugPending || slugDraft.trim().length === 0}
                className="bg-primary px-8 py-3 text-sm tracking-wide text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
              >
                {isSlugPending ? "Updating..." : "Update Slug"}
              </button>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  )
}
