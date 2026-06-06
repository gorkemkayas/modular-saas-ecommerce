"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { type FormEvent, type ReactNode, useMemo, useState, useTransition } from "react"

import {
  addProductMedia,
  addVariant,
  assignProductCategories,
  changeProductSlug,
  createInventoryItem,
  createSimpleProduct,
  createVariantProduct,
  getProductById,
  getProductBySlug,
  publishProduct,
  type AddProductMediaRequest,
  type AddVariantRequest,
  type AdminProductDto,
  type AttributeDefinitionDto,
  type BrandDto,
  type CategoryTreeNodeDto,
  type CreateInventoryItemRequest,
  uploadProductMediaFile,
  updateProductDetails,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"
import { resolveAdminBasePath } from "@/lib/admin-path"

type ProductTypeValue = "Simple" | "Variant"

type VariantDraft = {
  name: string
  sku: string
  stock: string
  reorderThreshold: string
  attributeValues: Record<string, string>
}

type ExistingMediaDraft = {
  url: string
  altText: string
  isMain: boolean
  mediaType: "Image" | "Video"
  productVariantId: string | null
}

type MediaUploadDraft = {
  file: File | null
  fileName: string
  previewUrl: string | null
  uploadedUrl: string | null
  altText: string
  isMain: boolean
  mediaType: "Image" | "Video" | null
  productVariantId: string | null
  variantDraftSku: string | null
  uploadProgress: number
  uploadStatus: "idle" | "uploading" | "uploaded" | "error"
}

type CategoryOption = {
  id: string
  name: string
  depth: number
}

const inputClassName =
  "w-full border border-border bg-background/80 px-4 py-3 text-sm transition focus:border-foreground/30 focus:outline-none focus:ring-1 focus:ring-foreground"
const textareaClassName = `${inputClassName} resize-none`
const sectionCardClassName =
  "border border-border bg-gradient-to-br from-background via-background to-secondary/30 p-6"

function flattenCategories(
  nodes: CategoryTreeNodeDto[],
  depth = 0,
): CategoryOption[] {
  return nodes.flatMap((node) => [
    {
      id: node.id,
      name: node.name,
      depth,
    },
    ...flattenCategories(node.children, depth + 1),
  ])
}

function parseOptionalNumber(value: string): number | null {
  const trimmedValue = value.trim()
  if (!trimmedValue) {
    return null
  }

  const parsedValue = Number(trimmedValue)
  return Number.isFinite(parsedValue) ? parsedValue : null
}

function normalizeVariants(variants: VariantDraft[]): VariantDraft[] {
  return variants.filter(
    (variant) => variant.sku.trim().length > 0 || variant.name.trim().length > 0,
  )
}

function syncVariantAttributeValues(
  attributeValues: Record<string, string>,
  selectedAttributeIds: string[],
): Record<string, string> {
  return selectedAttributeIds.reduce<Record<string, string>>((nextValues, attributeId) => {
    nextValues[attributeId] = attributeValues[attributeId] ?? ""
    return nextValues
  }, {})
}

function createEmptyVariantDraft(selectedAttributeIds: string[]): VariantDraft {
  return {
    name: "",
    sku: "",
    stock: "",
    reorderThreshold: "",
    attributeValues: syncVariantAttributeValues({}, selectedAttributeIds),
  }
}

function resolveMediaType(value: string | null | undefined): "Image" | "Video" {
  return value === "Video" ? "Video" : "Image"
}

function createEmptyMediaUploadDraft(isMain = false): MediaUploadDraft {
  return {
    file: null,
    fileName: "",
    previewUrl: null,
    uploadedUrl: null,
    altText: "",
    isMain,
    mediaType: null,
    productVariantId: null,
    variantDraftSku: null,
    uploadProgress: 0,
    uploadStatus: "idle",
  }
}

function resolveMediaTypeFromFile(file: File): "Image" | "Video" | null {
  if (file.type.startsWith("image/")) {
    return "Image"
  }

  if (file.type.startsWith("video/")) {
    return "Video"
  }

  const fileName = file.name.toLowerCase()

  if (
    fileName.endsWith(".jpg") ||
    fileName.endsWith(".jpeg") ||
    fileName.endsWith(".png") ||
    fileName.endsWith(".webp") ||
    fileName.endsWith(".gif") ||
    fileName.endsWith(".avif")
  ) {
    return "Image"
  }

  if (
    fileName.endsWith(".mp4") ||
    fileName.endsWith(".webm") ||
    fileName.endsWith(".mov") ||
    fileName.endsWith(".m4v") ||
    fileName.endsWith(".ogg")
  ) {
    return "Video"
  }

  return null
}

function SectionShell({
  eyebrow,
  title,
  description,
  children,
}: {
  eyebrow: string
  title: string
  description: string
  children: ReactNode
}) {
  return (
    <section className={sectionCardClassName}>
      <div className="flex flex-col gap-2 border-b border-border/60 pb-4">
        <p className="text-[11px] uppercase tracking-[0.28em] text-muted-foreground">
          {eyebrow}
        </p>
        <div className="space-y-1">
          <h2 className="text-xl font-medium tracking-tight">{title}</h2>
          <p className="max-w-2xl text-sm leading-6 text-muted-foreground">
            {description}
          </p>
        </div>
      </div>
      <div className="mt-5">{children}</div>
    </section>
  )
}

function FieldLabel({
  children,
  hint,
}: {
  children: ReactNode
  hint?: string
}) {
  return (
    <div className="mb-2 flex items-center justify-between gap-3">
      <label className="text-sm font-medium">{children}</label>
      {hint ? <span className="text-xs text-muted-foreground">{hint}</span> : null}
    </div>
  )
}

export function AdminProductEditor({
  brands,
  categories,
  attributes,
  initialProduct,
}: {
  brands: BrandDto[]
  categories: CategoryTreeNodeDto[]
  attributes: AttributeDefinitionDto[]
  initialProduct?: AdminProductDto | null
}) {
  const router = useRouter()
  const pathname = usePathname()
  const adminBasePath = resolveAdminBasePath(pathname)
  const [isPending, startTransition] = useTransition()
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const [name, setName] = useState(initialProduct?.name ?? "")
  const [slug, setSlug] = useState(initialProduct?.slug ?? "")
  const [brandId, setBrandId] = useState(initialProduct?.brandId ?? "")
  const [shortDescription, setShortDescription] = useState(
    initialProduct?.shortDescription ?? "",
  )
  const [description, setDescription] = useState(initialProduct?.description ?? "")
  const [productType, setProductType] = useState<ProductTypeValue>(
    initialProduct?.productType === "Variant" ? "Variant" : "Simple",
  )
  const [simpleSku, setSimpleSku] = useState(initialProduct?.sku ?? "")
  const [simpleStock, setSimpleStock] = useState("")
  const [simpleReorderThreshold, setSimpleReorderThreshold] = useState("")
  const variantDefiningAttributes = attributes.filter(
    (attribute) => attribute.isActive && attribute.isVariantDefining,
  )
  const initialSelectedVariantAttributeIds = Array.from(
    new Set(
      initialProduct?.variants.flatMap((variant) =>
        variant.attributeValues.map((attributeValue) => attributeValue.attributeDefinitionId),
      ) ?? [],
    ),
  ).filter((attributeDefinitionId) =>
    variantDefiningAttributes.some((attribute) => attribute.id === attributeDefinitionId),
  )
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<string[]>(
    initialProduct?.categories.map((category) => category.categoryId) ?? [],
  )
  const [mediaItems] = useState<ExistingMediaDraft[]>([
    ...(initialProduct?.mediaItems.map((mediaItem) => ({
      url: mediaItem.url,
      altText: mediaItem.altText ?? "",
      isMain: mediaItem.isMain,
      mediaType: resolveMediaType(mediaItem.mediaType),
      productVariantId: mediaItem.productVariantId ?? null,
    })) ?? []),
  ])
  const [newMediaItems, setNewMediaItems] = useState<MediaUploadDraft[]>([
    createEmptyMediaUploadDraft(!initialProduct?.mediaItems.length),
  ])
  const [selectedVariantAttributeIds, setSelectedVariantAttributeIds] = useState<string[]>(
    initialSelectedVariantAttributeIds,
  )
  const [newVariants, setNewVariants] = useState<VariantDraft[]>([
    createEmptyVariantDraft(initialSelectedVariantAttributeIds),
  ])
  const [publishAfterSave, setPublishAfterSave] = useState(false)

  const categoryOptions = useMemo(() => flattenCategories(categories), [categories])
  const selectedCategoryCount = selectedCategoryIds.length
  const existingVariantCount = initialProduct?.variants.length ?? 0
  const existingMediaCount = mediaItems.length
  const attributeNameById = useMemo(
    () =>
      new Map(
        variantDefiningAttributes.map((attribute) => [attribute.id, attribute.name] as const),
      ),
    [variantDefiningAttributes],
  )
  const selectableDraftVariants = useMemo(
    () =>
      normalizeVariants(newVariants)
        .filter((variant) => variant.sku.trim().length > 0)
        .map((variant) => ({
          sku: variant.sku.trim(),
          label: variant.name.trim() || variant.sku.trim(),
        })),
    [newVariants],
  )

  function toggleCategory(categoryId: string) {
    setSelectedCategoryIds((current) =>
      current.includes(categoryId)
        ? current.filter((id) => id !== categoryId)
        : [...current, categoryId],
    )
  }

  function toggleVariantAttribute(attributeDefinitionId: string) {
    setSelectedVariantAttributeIds((current) => {
      const nextAttributeIds = current.includes(attributeDefinitionId)
        ? current.filter((id) => id !== attributeDefinitionId)
        : [...current, attributeDefinitionId]

      setNewVariants((currentVariants) =>
        currentVariants.map((variant) => ({
          ...variant,
          attributeValues: syncVariantAttributeValues(
            variant.attributeValues,
            nextAttributeIds,
          ),
        })),
      )

      return nextAttributeIds
    })
  }

  function updateVariantDraft(
    index: number,
    updater: (variant: VariantDraft) => VariantDraft,
  ) {
    setNewVariants((current) =>
      current.map((currentVariant, currentIndex) =>
        currentIndex === index ? updater(currentVariant) : currentVariant,
      ),
    )
  }

  function validate(): string | null {
    if (!name.trim()) {
      return "Product name is required."
    }

    if (!slug.trim()) {
      return "Product slug is required."
    }

    if (productType === "Simple" && !simpleSku.trim()) {
      return "Simple products require a SKU."
    }

    if (productType === "Variant") {
      const variants = normalizeVariants(newVariants)
      if (!initialProduct && variants.length === 0) {
        return "Variant products require at least one variant."
      }

      if (variants.some((variant) => !variant.sku.trim())) {
        return "Every new variant requires a SKU."
      }

       if (variants.length > 0 && variantDefiningAttributes.length === 0) {
        return "Create at least one active variant-defining attribute before adding variants."
      }

      if (variants.length > 0 && selectedVariantAttributeIds.length === 0) {
        return "Select at least one variant-defining attribute for the new variants."
      }

      if (
        variants.some((variant) =>
          selectedVariantAttributeIds.some(
            (attributeDefinitionId) =>
              !(variant.attributeValues[attributeDefinitionId] ?? "").trim(),
          ),
        )
      ) {
        return "Every new variant needs a value for each selected variant-defining attribute."
      }
    }

    const mediaFilesToAttach = newMediaItems.filter((mediaItem) => mediaItem.uploadedUrl)

    if (
      mediaFilesToAttach.length > 0 &&
      existingMediaCount === 0 &&
      !mediaFilesToAttach.some((mediaItem) => mediaItem.isMain)
    ) {
      return "Select one main media item when uploading media."
    }

    if (newMediaItems.some((mediaItem) => mediaItem.file && !mediaItem.mediaType)) {
      return "Only image and video files are supported for product media."
    }

    if (newMediaItems.some((mediaItem) => mediaItem.uploadStatus === "uploading")) {
      return "Wait for all media uploads to finish before saving the product."
    }

    if (
      newMediaItems.some(
        (mediaItem) => mediaItem.file && !mediaItem.uploadedUrl && mediaItem.uploadStatus === "error",
      )
    ) {
      return "At least one media upload failed. Re-select the file before saving."
    }

    if (
      mediaFilesToAttach.some(
        (mediaItem) =>
          mediaItem.variantDraftSku &&
          !selectableDraftVariants.some((variant) => variant.sku === mediaItem.variantDraftSku),
      )
    ) {
      return "Every variant-specific media item must target a variant row with a SKU."
    }

    return null
  }

  function updateMediaTarget(index: number, value: string) {
    setNewMediaItems((current) =>
      current.map((mediaItem, currentIndex) => {
        if (currentIndex !== index) {
          return mediaItem
        }

        if (!value) {
          return {
            ...mediaItem,
            productVariantId: null,
            variantDraftSku: null,
          }
        }

        if (value.startsWith("existing:")) {
          return {
            ...mediaItem,
            productVariantId: value.slice("existing:".length),
            variantDraftSku: null,
          }
        }

        if (value.startsWith("draft:")) {
          return {
            ...mediaItem,
            productVariantId: null,
            variantDraftSku: value.slice("draft:".length),
          }
        }

        return mediaItem
      }),
    )
  }

  async function handleMediaFileChange(index: number, file: File | null) {
    setNewMediaItems((current) =>
      current.map((mediaItem, currentIndex) => {
        if (currentIndex !== index) {
          return mediaItem
        }

        if (mediaItem.previewUrl) {
          URL.revokeObjectURL(mediaItem.previewUrl)
        }

        if (!file) {
          return {
            ...mediaItem,
            file: null,
            fileName: "",
            previewUrl: null,
            uploadedUrl: null,
            mediaType: null,
            productVariantId: null,
            variantDraftSku: null,
            uploadProgress: 0,
            uploadStatus: "idle",
          }
        }

        return {
          ...mediaItem,
          file,
          fileName: file.name,
          previewUrl: URL.createObjectURL(file),
          uploadedUrl: null,
          mediaType: resolveMediaTypeFromFile(file),
          uploadProgress: 0,
          uploadStatus: "uploading",
        }
      }),
    )

    if (!file) {
      return
    }

    const resolvedMediaType = resolveMediaTypeFromFile(file)

    if (!resolvedMediaType) {
      setNewMediaItems((current) =>
        current.map((mediaItem, currentIndex) =>
          currentIndex === index
            ? {
                ...mediaItem,
                uploadStatus: "error",
              }
            : mediaItem,
        ),
      )
      return
    }

    try {
      const uploadedFile = await uploadProductMediaFile(file, (progress) => {
        setNewMediaItems((current) =>
          current.map((mediaItem, currentIndex) =>
            currentIndex === index
              ? {
                  ...mediaItem,
                  uploadProgress: progress,
                }
              : mediaItem,
          ),
        )
      })

      setNewMediaItems((current) =>
        current.map((mediaItem, currentIndex) =>
          currentIndex === index
            ? {
                ...mediaItem,
                uploadedUrl: uploadedFile.url,
                mediaType: resolveMediaType(uploadedFile.mediaType),
                uploadProgress: 100,
                uploadStatus: "uploaded",
              }
            : mediaItem,
        ),
      )
    } catch (uploadError) {
      setNewMediaItems((current) =>
        current.map((mediaItem, currentIndex) =>
          currentIndex === index
            ? {
                ...mediaItem,
                uploadedUrl: null,
                uploadStatus: "error",
              }
            : mediaItem,
        ),
      )
      setError(
        getApiErrorMessage(
          uploadError,
          "The selected media file could not be uploaded. Please try again.",
        ),
      )
    }
  }

  function buildPendingMedia(variantIdBySku?: Map<string, string>) {
    const uploadedMedia: AddProductMediaRequest[] = []

    for (const mediaItem of newMediaItems) {
      if (!mediaItem.uploadedUrl) {
        continue
      }

      let resolvedProductVariantId = mediaItem.productVariantId

      if (!resolvedProductVariantId && mediaItem.variantDraftSku) {
        resolvedProductVariantId = variantIdBySku?.get(mediaItem.variantDraftSku) ?? null

        if (!resolvedProductVariantId) {
          throw new Error(
            `The selected variant media target '${mediaItem.variantDraftSku}' could not be resolved.`,
          )
        }
      }

      uploadedMedia.push({
        mediaType: mediaItem.mediaType as "Image" | "Video",
        url: mediaItem.uploadedUrl,
        altText: mediaItem.altText.trim() || null,
        isMain: mediaItem.isMain,
        sortOrder: uploadedMedia.length,
        productVariantId: resolvedProductVariantId,
      })
    }

    return uploadedMedia
  }

  async function submitCreateFlow() {
    if (productType === "Simple") {
      await createSimpleProduct({
        name: name.trim(),
        slug: slug.trim(),
        sku: simpleSku.trim(),
        shortDescription: shortDescription.trim() || null,
        description: description.trim() || null,
        brandId: brandId || null,
        categoryIds: selectedCategoryIds,
      })
    } else {
      await createVariantProduct({
        name: name.trim(),
        slug: slug.trim(),
        shortDescription: shortDescription.trim() || null,
        description: description.trim() || null,
        brandId: brandId || null,
        categoryIds: selectedCategoryIds,
      })
    }

    const createdProduct = await getProductBySlug(slug.trim())

    let createdVariantIdBySku: Map<string, string> | undefined

    if (productType === "Simple") {
      const inventoryRequest: CreateInventoryItemRequest = {
        productId: createdProduct.id,
        productVariantId: null,
        initialOnHandQuantity: Number(simpleStock || "0"),
        reorderThreshold: parseOptionalNumber(simpleReorderThreshold),
      }

      if (
        inventoryRequest.initialOnHandQuantity > 0 ||
        inventoryRequest.reorderThreshold !== null
      ) {
        await createInventoryItem(inventoryRequest)
      }
    } else {
      const variantsToCreate = normalizeVariants(newVariants)
      for (const [index, variant] of variantsToCreate.entries()) {
        const request: AddVariantRequest = {
          sku: variant.sku.trim(),
          name: variant.name.trim() || null,
          sortOrder: index,
          attributeValues: selectedVariantAttributeIds.map((attributeDefinitionId) => ({
            attributeDefinitionId,
            value: (variant.attributeValues[attributeDefinitionId] ?? "").trim(),
          })),
        }

        await addVariant(createdProduct.id, request)
      }

      const refreshedProduct = await getProductById(createdProduct.id)
      const variantMap = new Map(
        refreshedProduct.variants.map((variant) => [variant.sku, variant.id]),
      )
      createdVariantIdBySku = variantMap

      for (const variant of variantsToCreate) {
        const variantId = variantMap.get(variant.sku.trim())
        if (!variantId) {
          continue
        }

        const inventoryRequest: CreateInventoryItemRequest = {
          productId: refreshedProduct.id,
          productVariantId: variantId,
          initialOnHandQuantity: Number(variant.stock || "0"),
          reorderThreshold: parseOptionalNumber(variant.reorderThreshold),
        }

        if (
          inventoryRequest.initialOnHandQuantity > 0 ||
          inventoryRequest.reorderThreshold !== null
        ) {
          await createInventoryItem(inventoryRequest)
        }
      }
    }

    const mediaToCreate = buildPendingMedia(createdVariantIdBySku)
    for (const [index, mediaItem] of mediaToCreate.entries()) {
      await addProductMedia(createdProduct.id, {
        ...mediaItem,
        sortOrder: index,
      })
    }

    if (publishAfterSave) {
      await publishProduct(createdProduct.id)
    }

    router.push(`${adminBasePath}/products/${createdProduct.id}`)
    router.refresh()
  }

  async function submitEditFlow() {
    if (!initialProduct) {
      return
    }

    await updateProductDetails(initialProduct.id, {
      name: name.trim(),
      shortDescription: shortDescription.trim() || null,
      description: description.trim() || null,
      brandId: brandId || null,
    })

    if (slug.trim() !== initialProduct.slug) {
      await changeProductSlug(initialProduct.id, { slug: slug.trim() })
    }

    await assignProductCategories(initialProduct.id, {
      categoryIds: selectedCategoryIds,
    })

    const variantsToCreate = normalizeVariants(newVariants)
    let variantIdBySku: Map<string, string> | undefined

    if (initialProduct.productType === "Variant") {
      for (const [index, variant] of variantsToCreate.entries()) {
        await addVariant(initialProduct.id, {
          sku: variant.sku.trim(),
          name: variant.name.trim() || null,
          sortOrder: initialProduct.variants.length + index,
          attributeValues: selectedVariantAttributeIds.map((attributeDefinitionId) => ({
            attributeDefinitionId,
            value: (variant.attributeValues[attributeDefinitionId] ?? "").trim(),
          })),
        })
      }

      if (variantsToCreate.length) {
        const refreshedProduct = await getProductById(initialProduct.id)
        const variantMap = new Map(
          refreshedProduct.variants.map((variant) => [variant.sku, variant.id]),
        )
        variantIdBySku = variantMap

        for (const variant of variantsToCreate) {
          const variantId = variantMap.get(variant.sku.trim())
          if (!variantId) {
            continue
          }

          const inventoryRequest: CreateInventoryItemRequest = {
            productId: refreshedProduct.id,
            productVariantId: variantId,
            initialOnHandQuantity: Number(variant.stock || "0"),
            reorderThreshold: parseOptionalNumber(variant.reorderThreshold),
          }

          if (
            inventoryRequest.initialOnHandQuantity > 0 ||
            inventoryRequest.reorderThreshold !== null
          ) {
            await createInventoryItem(inventoryRequest)
          }
        }
      }
    }

    const mediaToCreate = buildPendingMedia(variantIdBySku)
    for (const [index, mediaItem] of mediaToCreate.entries()) {
      await addProductMedia(initialProduct.id, {
        ...mediaItem,
        sortOrder: initialProduct.mediaItems.length + index,
      })
    }

    if (publishAfterSave && !initialProduct.isPublished) {
      await publishProduct(initialProduct.id)
    }

    router.push(`${adminBasePath}/products/${initialProduct.id}`)
    router.refresh()
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setMessage(null)

    const validationError = validate()
    if (validationError) {
      setError(validationError)
      return
    }

    startTransition(async () => {
      try {
        if (initialProduct) {
          await submitEditFlow()
          setMessage("Product updated successfully.")
          return
        }

        await submitCreateFlow()
      } catch (saveError) {
        setNewMediaItems((current) =>
          current.map((item) =>
            item.uploadStatus === "uploading"
              ? { ...item, uploadStatus: "error" }
              : item,
          ),
        )
        setError(
          getApiErrorMessage(
            saveError,
            initialProduct
              ? "The product could not be updated."
              : "The product could not be created.",
          ),
        )
      }
    })
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <div className="border border-border bg-gradient-to-br from-background via-background to-secondary/40 p-6">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-4">
            <Link
              href={`${adminBasePath}/products`}
              className="inline-flex items-center gap-2 border border-border bg-background/80 px-4 py-2 text-sm text-muted-foreground transition hover:text-foreground"
            >
              Back to products
            </Link>
            <div>
              <p className="text-[11px] uppercase tracking-[0.32em] text-muted-foreground">
                Catalog workspace
              </p>
              <h1 className="mt-2 text-3xl font-medium tracking-tight">
                {initialProduct ? "Refine product presentation" : "Create a premium product record"}
              </h1>
              <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground">
                This flow writes catalog data to the backend, lets us seed inventory on
                creation, and keeps pricing in price lists where it belongs.
              </p>
            </div>
          </div>

          <div className="grid gap-3 sm:grid-cols-3 lg:min-w-[360px]">
            <div className="border border-border bg-background/75 p-4">
              <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                Mode
              </p>
              <p className="mt-2 text-base font-medium">
                {initialProduct ? "Edit existing" : "Create new"}
              </p>
            </div>
            <div className="border border-border bg-background/75 p-4">
              <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                Categories
              </p>
              <p className="mt-2 text-base font-medium">{selectedCategoryCount}</p>
            </div>
            <div className="border border-border bg-background/75 p-4">
              <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                Media
              </p>
              <p className="mt-2 text-base font-medium">{existingMediaCount}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <div className="border border-border bg-background/70 p-5">
          <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
            Pricing model
          </p>
          <p className="mt-2 text-sm leading-6 text-muted-foreground">
            Product creation does not store sale price. After saving, attach price
            entries from the price list area.
          </p>
        </div>
        <div className="border border-border bg-background/70 p-5">
          <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
            Inventory seeding
          </p>
          <p className="mt-2 text-sm leading-6 text-muted-foreground">
            Simple products can start with stock immediately. Variant products can seed
            stock per new variant row.
          </p>
        </div>
      </div>

      <form className="space-y-6" onSubmit={handleSubmit}>
        <SectionShell
          eyebrow="Identity"
          title="Catalog identity"
          description="Define the product headline, routing slug, brand association, and the core product type the backend will store."
        >
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="md:col-span-2">
              <FieldLabel hint="Customer-facing title">Product name</FieldLabel>
              <input
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Minimal sneaker"
                className={inputClassName}
              />
            </div>
            <div>
              <FieldLabel hint="Used in the URL">Slug</FieldLabel>
              <input
                value={slug}
                onChange={(event) => setSlug(event.target.value)}
                placeholder="minimal-sneaker"
                className={inputClassName}
              />
            </div>
            <div>
              <FieldLabel>Brand</FieldLabel>
              <select
                value={brandId}
                onChange={(event) => setBrandId(event.target.value)}
                className={inputClassName}
              >
                <option value="">No brand</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>
                    {brand.name}
                  </option>
                ))}
              </select>
            </div>
            {!initialProduct ? (
              <>
                <div>
                  <FieldLabel hint="Controls inventory setup">Product type</FieldLabel>
                  <select
                    value={productType}
                    onChange={(event) =>
                      setProductType(event.target.value as ProductTypeValue)
                    }
                    className={inputClassName}
                  >
                    <option value="Simple">Simple</option>
                    <option value="Variant">Variant</option>
                  </select>
                </div>
                {productType === "Simple" ? (
                  <div>
                    <FieldLabel hint="Required for simple products">Base SKU</FieldLabel>
                    <input
                      value={simpleSku}
                      onChange={(event) => setSimpleSku(event.target.value)}
                      placeholder="SKU-001"
                      className={inputClassName}
                    />
                  </div>
                ) : null}
              </>
            ) : (
              <div>
                <FieldLabel>Product type</FieldLabel>
                <input
                  value={initialProduct.productType}
                  disabled
                  className={`${inputClassName} text-muted-foreground`}
                />
              </div>
            )}
            <div className="md:col-span-2">
              <FieldLabel hint="Short supporting line">Short description</FieldLabel>
              <textarea
                rows={2}
                value={shortDescription}
                onChange={(event) => setShortDescription(event.target.value)}
                placeholder="A clean, low-profile staple with premium materials."
                className={textareaClassName}
              />
            </div>
            <div className="md:col-span-2">
              <FieldLabel hint="Long-form merchandising copy">Description</FieldLabel>
              <textarea
                rows={5}
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                placeholder="Describe the product story, materials, and why it belongs in the catalog."
                className={textareaClassName}
              />
            </div>
          </div>
        </SectionShell>

        <SectionShell
          eyebrow="Structure"
          title="Category placement"
          description="Place the product in one or more storefront categories so navigation, filtering, and collection pages stay coherent."
        >
          <div className="mb-4 flex items-center justify-between gap-4">
            <p className="text-sm text-muted-foreground">
              {selectedCategoryCount
                ? `${selectedCategoryCount} categories selected`
                : "No categories selected yet"}
            </p>
            <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
              {categoryOptions.length} available
            </p>
          </div>
          <div className="grid gap-3 sm:grid-cols-2">
            {categoryOptions.map((category) => {
              const isSelected = selectedCategoryIds.includes(category.id)

              return (
                <label
                  key={category.id}
                  className={`flex items-center gap-3 border p-4 text-sm transition ${
                    isSelected
                      ? "border-foreground/20 bg-secondary/60"
                      : "border-border/70 bg-background/70 hover:bg-secondary/40"
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={isSelected}
                    onChange={() => toggleCategory(category.id)}
                    className="h-4 w-4"
                  />
                  <div className="min-w-0">
                    <p
                      className="truncate font-medium"
                      style={{ paddingLeft: `${category.depth * 12}px` }}
                    >
                      {category.name}
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      Level {category.depth + 1}
                    </p>
                  </div>
                </label>
              )
            })}
          </div>
        </SectionShell>

        {!initialProduct && productType === "Simple" ? (
          <SectionShell
            eyebrow="Inventory"
            title="Initial inventory"
            description="Seed first on-hand quantity and reorder guidance for simple products right from the creation flow."
          >
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <FieldLabel hint="Optional">Initial on-hand quantity</FieldLabel>
                <input
                  type="number"
                  value={simpleStock}
                  onChange={(event) => setSimpleStock(event.target.value)}
                  placeholder="0"
                  className={inputClassName}
                />
              </div>
              <div>
                <FieldLabel hint="Optional">Reorder threshold</FieldLabel>
                <input
                  type="number"
                  value={simpleReorderThreshold}
                  onChange={(event) => setSimpleReorderThreshold(event.target.value)}
                  placeholder="0"
                  className={inputClassName}
                />
              </div>
            </div>
          </SectionShell>
        ) : null}

        {productType === "Variant" || initialProduct?.productType === "Variant" ? (
          <SectionShell
            eyebrow="Variants"
            title="Variant setup"
            description="Choose the variant-defining attributes first, then add option rows with SKU, attribute values, and optional stock."
          >
            <div className="mb-5 space-y-3 border border-border bg-background/70 p-4">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="text-sm font-medium">Variant-defining attributes</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Backend yeni variant oluştururken en az bir aktif variant attribute
                    istiyor.
                  </p>
                </div>
                <span className="text-xs text-muted-foreground">
                  {selectedVariantAttributeIds.length} selected
                </span>
              </div>
              {variantDefiningAttributes.length ? (
                <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                  {variantDefiningAttributes.map((attribute) => {
                    const isSelected = selectedVariantAttributeIds.includes(attribute.id)

                    return (
                      <label
                        key={attribute.id}
                        className={`flex cursor-pointer items-start gap-3 border p-4 transition ${
                          isSelected
                            ? "border-foreground bg-secondary/40"
                            : "border-border bg-background/80 hover:bg-secondary/20"
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => toggleVariantAttribute(attribute.id)}
                          className="mt-1 h-4 w-4 border-border"
                        />
                        <div className="min-w-0">
                          <p className="text-sm font-medium">{attribute.name}</p>
                          <p className="mt-1 text-xs uppercase tracking-[0.2em] text-muted-foreground">
                            {attribute.code}
                          </p>
                        </div>
                      </label>
                    )
                  })}
                </div>
              ) : (
                <div className="border border-dashed border-border bg-background/80 p-4 text-sm text-muted-foreground">
                  No active variant-defining attributes yet. Create one in the Attributes
                  area first, then come back to add variants.
                </div>
              )}
            </div>
            {existingVariantCount ? (
              <div className="mb-5 grid gap-3 md:grid-cols-2">
                {initialProduct?.variants.map((variant) => (
                  <div
                    key={variant.id}
                    className="border border-border bg-background/70 p-4"
                  >
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="text-sm font-medium">{variant.name ?? variant.sku}</p>
                        <p className="mt-1 text-xs text-muted-foreground">{variant.sku}</p>
                      </div>
                      <span className="bg-secondary px-3 py-1 text-[11px] uppercase tracking-[0.22em] text-muted-foreground">
                        {variant.isActive ? "Active" : "Inactive"}
                      </span>
                    </div>
                    {variant.attributeValues.length ? (
                      <div className="mt-3 flex flex-wrap gap-2">
                        {variant.attributeValues.map((attributeValue) => (
                          <span
                            key={`${variant.id}-${attributeValue.attributeDefinitionId}`}
                            className="border border-border bg-background px-2 py-1 text-xs text-muted-foreground"
                          >
                            {attributeNameById.get(attributeValue.attributeDefinitionId) ??
                              "Attribute"}
                            : {attributeValue.value}
                          </span>
                        ))}
                      </div>
                    ) : null}
                  </div>
                ))}
              </div>
            ) : null}
            <div className="space-y-3">
              {newVariants.map((variant, index) => (
                <div
                  key={index}
                  className="space-y-3 border border-border bg-background/70 p-4"
                >
                  <div className="grid gap-3 md:grid-cols-4">
                    <input
                      placeholder="Variant name"
                      value={variant.name}
                      onChange={(event) =>
                        updateVariantDraft(index, (currentVariant) => ({
                          ...currentVariant,
                          name: event.target.value,
                        }))
                      }
                      className={inputClassName}
                    />
                    <input
                      placeholder="Variant SKU"
                      value={variant.sku}
                      onChange={(event) =>
                        updateVariantDraft(index, (currentVariant) => ({
                          ...currentVariant,
                          sku: event.target.value,
                        }))
                      }
                      className={inputClassName}
                    />
                    <input
                      type="number"
                      placeholder="Initial stock"
                      value={variant.stock}
                      onChange={(event) =>
                        updateVariantDraft(index, (currentVariant) => ({
                          ...currentVariant,
                          stock: event.target.value,
                        }))
                      }
                      className={inputClassName}
                    />
                    <input
                      type="number"
                      placeholder="Reorder threshold"
                      value={variant.reorderThreshold}
                      onChange={(event) =>
                        updateVariantDraft(index, (currentVariant) => ({
                          ...currentVariant,
                          reorderThreshold: event.target.value,
                        }))
                      }
                      className={inputClassName}
                    />
                  </div>
                  {selectedVariantAttributeIds.length ? (
                    <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                      {selectedVariantAttributeIds.map((attributeDefinitionId) => (
                        <div key={`${index}-${attributeDefinitionId}`}>
                          <FieldLabel hint="Required">
                            {attributeNameById.get(attributeDefinitionId) ?? "Attribute"}
                          </FieldLabel>
                          <input
                            placeholder={`Value for ${
                              attributeNameById.get(attributeDefinitionId) ?? "attribute"
                            }`}
                            value={variant.attributeValues[attributeDefinitionId] ?? ""}
                            onChange={(event) =>
                              updateVariantDraft(index, (currentVariant) => ({
                                ...currentVariant,
                                attributeValues: {
                                  ...currentVariant.attributeValues,
                                  [attributeDefinitionId]: event.target.value,
                                },
                              }))
                            }
                            className={inputClassName}
                          />
                        </div>
                      ))}
                    </div>
                  ) : null}
                </div>
              ))}
              <button
                type="button"
                onClick={() =>
                  setNewVariants((current) => [
                    ...current,
                    createEmptyVariantDraft(selectedVariantAttributeIds),
                  ])
                }
                className="border border-border px-5 py-3 text-sm font-medium transition hover:bg-secondary"
              >
                Add variant row
              </button>
            </div>
          </SectionShell>
        ) : null}

        <SectionShell
          eyebrow="Media"
          title="Product media"
          description="Upload product images or videos from your device. Files are sent to Cloudinary first, then the returned URL is saved to the product."
        >
          {mediaItems.length ? (
            <div className="mb-5 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {mediaItems.map((mediaItem, index) => (
                <div
                  key={`${mediaItem.url}-${index}`}
                  className="overflow-hidden border border-border bg-background/70"
                >
                  <div className="aspect-[4/3] bg-secondary/40">
                    {mediaItem.mediaType === "Video" ? (
                      <video
                        src={mediaItem.url}
                        controls
                        preload="metadata"
                        className="h-full w-full object-cover"
                      />
                    ) : (
                      <img
                        src={mediaItem.url}
                        alt={mediaItem.altText || `Product media ${index + 1}`}
                        className="h-full w-full object-cover"
                      />
                    )}
                  </div>
                  <div className="space-y-2 p-4">
                    <div className="flex items-center justify-between gap-3">
                      <p className="truncate text-sm font-medium">
                        {mediaItem.altText || "Untitled media"}
                      </p>
                      {mediaItem.isMain ? (
                        <span className="bg-primary/10 px-3 py-1 text-[11px] uppercase tracking-[0.22em] text-primary">
                          Main
                        </span>
                      ) : null}
                    </div>
                    <p className="text-[11px] uppercase tracking-[0.22em] text-muted-foreground">
                      {mediaItem.mediaType}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {mediaItem.productVariantId
                        ? `Variant: ${
                            initialProduct?.variants.find(
                              (variant) => variant.id === mediaItem.productVariantId,
                            )?.name ??
                            initialProduct?.variants.find(
                              (variant) => variant.id === mediaItem.productVariantId,
                            )?.sku ??
                            mediaItem.productVariantId
                          }`
                        : mediaItem.variantDraftSku
                          ? `Draft variant: ${
                              selectableDraftVariants.find(
                                (variant) => variant.sku === mediaItem.variantDraftSku,
                              )?.label ?? mediaItem.variantDraftSku
                            }`
                        : "Applies to the main product"}
                    </p>
                    <p className="break-all text-xs text-muted-foreground">{mediaItem.url}</p>
                  </div>
                </div>
              ))}
            </div>
          ) : null}
          <div className="space-y-3">
            {newMediaItems.map((mediaItem, index) => (
              <div
                key={index}
                className="grid gap-4 border border-border bg-background/70 p-4 md:grid-cols-[1.4fr_1fr_auto]"
              >
                <div className="space-y-3">
                  <div
                    onDragOver={(event) => event.preventDefault()}
                    onDrop={(event) => {
                      event.preventDefault()
                      handleMediaFileChange(index, event.dataTransfer.files?.[0] ?? null)
                    }}
                    className="space-y-3 border border-dashed border-border p-4"
                  >
                    <input
                      type="file"
                      accept="image/*,video/*"
                      onChange={(event) =>
                        handleMediaFileChange(index, event.target.files?.[0] ?? null)
                      }
                      className={inputClassName}
                    />
                    <p className="text-xs text-muted-foreground">
                      Drag and drop an image or video here, or choose a file manually.
                    </p>
                  </div>
                  {mediaItem.previewUrl ? (
                    <div className="overflow-hidden border border-border bg-background">
                      <div className="aspect-[4/3] bg-secondary/40">
                        {mediaItem.mediaType === "Video" ? (
                          <video
                            src={mediaItem.previewUrl}
                            controls
                            preload="metadata"
                            className="h-full w-full object-cover"
                          />
                        ) : (
                          <img
                            src={mediaItem.previewUrl}
                            alt={mediaItem.altText || mediaItem.fileName || "Selected media"}
                            className="h-full w-full object-cover"
                          />
                        )}
                      </div>
                      <div className="space-y-1 p-3 text-xs text-muted-foreground">
                        <p className="font-medium text-foreground">
                          {mediaItem.fileName || "No file selected"}
                        </p>
                        <p>{mediaItem.mediaType ?? "Image or video"}</p>
                        <p>
                          {mediaItem.uploadStatus === "uploading"
                            ? `Uploading ${mediaItem.uploadProgress}%`
                            : mediaItem.uploadStatus === "uploaded"
                              ? "Ready to save"
                              : mediaItem.uploadStatus === "error"
                                ? "Upload failed"
                                : "Waiting to upload"}
                        </p>
                        <div className="h-2 bg-secondary">
                          <div
                            className="h-full bg-primary transition-all"
                            style={{ width: `${mediaItem.uploadProgress}%` }}
                          />
                        </div>
                      </div>
                    </div>
                  ) : (
                    <div className="border border-dashed border-border p-4 text-xs text-muted-foreground">
                      Choose an image or video from your computer. The file will be
                      uploaded to Cloudinary before the media URL is saved.
                    </div>
                  )}
                </div>
                <div className="space-y-3">
                  <input
                    placeholder="Alt text"
                    value={mediaItem.altText}
                    onChange={(event) =>
                      setNewMediaItems((current) =>
                        current.map((currentMedia, currentIndex) =>
                          currentIndex === index
                            ? { ...currentMedia, altText: event.target.value }
                            : currentMedia,
                        ),
                      )
                    }
                    className={inputClassName}
                  />
                  {initialProduct?.variants.length || selectableDraftVariants.length ? (
                    <select
                      value={
                        mediaItem.productVariantId
                          ? `existing:${mediaItem.productVariantId}`
                          : mediaItem.variantDraftSku
                            ? `draft:${mediaItem.variantDraftSku}`
                            : ""
                      }
                      onChange={(event) => updateMediaTarget(index, event.target.value)}
                      className={inputClassName}
                    >
                      <option value="">Main product media</option>
                      {initialProduct?.variants.map((variant) => (
                        <option key={variant.id} value={`existing:${variant.id}`}>
                          {variant.name ?? variant.sku}
                        </option>
                      ))}
                      {selectableDraftVariants.map((variant) => (
                        <option key={`draft-${variant.sku}`} value={`draft:${variant.sku}`}>
                          New variant row: {variant.label}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <div className="border border-border bg-background px-4 py-3 text-xs text-muted-foreground">
                      This media will be attached at the product level.
                    </div>
                  )}
                </div>
                <label className="flex min-h-[54px] items-center justify-center gap-3 border border-border bg-background/80 px-4 text-sm">
                  <input
                    type="checkbox"
                    checked={mediaItem.isMain}
                    onChange={() =>
                      setNewMediaItems((current) =>
                        current.map((currentMedia, currentIndex) => ({
                          ...currentMedia,
                          isMain: currentIndex === index,
                        })),
                      )
                    }
                  />
                  Main media
                </label>
              </div>
            ))}
            <button
              type="button"
              onClick={() =>
                setNewMediaItems((current) => [
                  ...current,
                  createEmptyMediaUploadDraft(false),
                ])
              }
              className="border border-border px-5 py-3 text-sm font-medium transition hover:bg-secondary"
            >
              Add media row
            </button>
          </div>
        </SectionShell>

        <div className="border border-border bg-gradient-to-br from-background via-background to-secondary/30 p-6">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="text-[11px] uppercase tracking-[0.28em] text-muted-foreground">
                Publish preference
              </p>
              <h2 className="mt-2 text-xl font-medium tracking-tight">
                Decide whether this product should go live after save
              </h2>
              <p className="mt-2 text-sm leading-6 text-muted-foreground">
                You can keep the product private for additional pricing or inventory
                work, or publish immediately once the catalog record is ready.
              </p>
            </div>

            <label className="flex cursor-pointer items-center gap-4 border border-border bg-background/80 px-5 py-4">
              <input
                type="checkbox"
                checked={publishAfterSave}
                onChange={(event) => setPublishAfterSave(event.target.checked)}
                className="h-4 w-4"
              />
              <div>
                <p className="text-sm font-medium">Publish after save</p>
                <p className="text-xs text-muted-foreground">
                  {publishAfterSave ? "Will publish immediately" : "Stay private for now"}
                </p>
              </div>
            </label>
          </div>
        </div>

        {message ? (
          <div className="border border-border bg-secondary/40 px-6 py-4 text-sm">
            {message}
          </div>
        ) : null}

        {error ? (
          <div className="border border-destructive/30 bg-destructive/5 px-6 py-4 text-sm text-destructive">
            {error}
          </div>
        ) : null}

        <div className="flex flex-col gap-3 border border-border bg-background/80 p-5 sm:flex-row sm:items-center sm:justify-between">
          <div className="text-sm text-muted-foreground">
            {initialProduct
              ? "Edits are written directly to the backend catalog record."
              : "A new catalog record will be created and then enriched with inventory and media."}
          </div>
          <div className="flex items-center gap-3">
            <Link
              href={`${adminBasePath}/products`}
              className="px-5 py-3 text-sm font-medium transition hover:bg-secondary"
            >
              Cancel
            </Link>
            <button
              type="submit"
              disabled={isPending}
              className="bg-primary px-6 py-3 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 disabled:opacity-60"
            >
              {isPending
                ? initialProduct
                  ? "Saving..."
                  : "Creating..."
                : initialProduct
                  ? "Save changes"
                  : "Create product"}
            </button>
          </div>
        </div>
      </form>
    </div>
  )
}
