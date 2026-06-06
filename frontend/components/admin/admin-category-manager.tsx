"use client"

import { useEffect, useMemo, useState, useTransition } from "react"
import { useRouter } from "next/navigation"

import {
  changeCategoryParent,
  updateCategory,
  uploadCategoryImageFile,
} from "@/lib/api/admin"
import { getApiErrorMessage } from "@/lib/api/error-message"

type CategoryRow = {
  id: string
  name: string
  slug: string
  depth: number
  isActive: boolean
  childCount: number
  description: string | null
  imageUrl: string | null
  parentCategoryId: string | null
  sortOrder: number
}

type CategoryOption = {
  id: string
  name: string
  depth: number
}

type AdminCategoryManagerProps = {
  rows: CategoryRow[]
  categoryOptions: CategoryOption[]
}

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

export function AdminCategoryManager({
  rows,
  categoryOptions,
}: AdminCategoryManagerProps) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null)
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

  const editingCategory = useMemo(
    () => rows.find((row) => row.id === editingCategoryId) ?? null,
    [rows, editingCategoryId],
  )

  useEffect(() => {
    return () => {
      if (imagePreviewUrl?.startsWith("blob:")) {
        URL.revokeObjectURL(imagePreviewUrl)
      }
    }
  }, [imagePreviewUrl])

  useEffect(() => {
    if (!editingCategory) {
      return
    }

    setName(editingCategory.name)
    setSlug(editingCategory.slug)
    setDescription(editingCategory.description ?? "")
    setParentCategoryId(editingCategory.parentCategoryId ?? "")
    setSortOrder(String(editingCategory.sortOrder))
    setImageUrl(editingCategory.imageUrl)
    setImagePreviewUrl(editingCategory.imageUrl)
    setImageFileName(null)
    setImageUploadProgress(0)
    setError(null)
  }, [editingCategory])

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
      setImageUrl(editingCategory?.imageUrl ?? null)
      setError(getApiErrorMessage(uploadError, "Category image upload failed."))
    } finally {
      setIsImageUploading(false)
      event.target.value = ""
    }
  }

  function resetEditor() {
    setEditingCategoryId(null)
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
    setError(null)
  }

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()

    if (!editingCategory) {
      return
    }

    setError(null)

    startTransition(async () => {
      try {
        await updateCategory(editingCategory.id, {
          name: name.trim(),
          slug: slug.trim(),
          description: description.trim() || null,
          imageUrl,
          sortOrder: Number(sortOrder || "0"),
        })

        const normalizedParentId = parentCategoryId || null
        if (normalizedParentId !== editingCategory.parentCategoryId) {
          await changeCategoryParent(editingCategory.id, {
            parentCategoryId: normalizedParentId,
          })
        }

        resetEditor()
        router.refresh()
      } catch (submitError) {
        setError(getApiErrorMessage(submitError, "Category update failed."))
      }
    })
  }

  return (
    <div className="space-y-4">
      <div className="border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-border bg-secondary/50">
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Category
                </th>
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Image
                </th>
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Slug
                </th>
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Children
                </th>
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Status
                </th>
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Description
                </th>
                <th className="p-4 text-left text-xs uppercase tracking-wider text-muted-foreground">
                  Action
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {rows.map((category) => (
                <tr key={category.id} className="hover:bg-secondary/30">
                  <td className="p-4 text-sm">
                    <span style={{ paddingLeft: `${category.depth * 16}px` }}>
                      {category.name}
                    </span>
                  </td>
                  <td className="p-4 text-sm text-muted-foreground">
                    {category.imageUrl ? (
                      <img
                        src={category.imageUrl}
                        alt={category.name}
                        className="h-14 w-11 object-cover"
                      />
                    ) : (
                      "No image"
                    )}
                  </td>
                  <td className="p-4 text-sm text-muted-foreground">/{category.slug}</td>
                  <td className="p-4 text-sm">{category.childCount}</td>
                  <td className="p-4 text-sm">
                    {category.isActive ? "Active" : "Inactive"}
                  </td>
                  <td className="p-4 text-sm text-muted-foreground">
                    {category.description ?? "No description"}
                  </td>
                  <td className="p-4 text-sm">
                    <button
                      type="button"
                      onClick={() => setEditingCategoryId(category.id)}
                      className="border border-border px-3 py-2 text-xs uppercase tracking-[0.2em] transition-colors hover:border-foreground"
                    >
                      Edit
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {editingCategory ? (
        <form onSubmit={handleSubmit} className="space-y-4 border border-border p-4">
          <div className="flex items-center justify-between gap-4">
            <div>
              <h2 className="text-sm uppercase tracking-[0.2em] text-muted-foreground">
                Edit Category
              </h2>
              <p className="mt-1 text-sm">{editingCategory.name}</p>
            </div>
            <button
              type="button"
              onClick={resetEditor}
              className="text-xs uppercase tracking-[0.2em] text-muted-foreground transition-colors hover:text-foreground"
            >
              Close
            </button>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <input
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="Name"
              className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
            />
            <input
              value={slug}
              onChange={(event) => setSlug(event.target.value)}
              placeholder="slug"
              className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
            />
            <input
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              placeholder="Description"
              className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
            />
            <input
              type="number"
              value={sortOrder}
              onChange={(event) => setSortOrder(event.target.value)}
              placeholder="Sort Order"
              className="bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
            />
            <select
              value={parentCategoryId}
              onChange={(event) => setParentCategoryId(event.target.value)}
              className="md:col-span-2 bg-secondary px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-foreground"
            >
              <option value="">Top level</option>
              {categoryOptions
                .filter((category) => category.id !== editingCategory.id)
                .map((category) => (
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
                Upload a new image to replace the current category visual.
              </p>
              {imagePreviewUrl ? (
                <div className="flex items-center gap-4 border border-border bg-secondary/20 p-3">
                  <img
                    src={imagePreviewUrl}
                    alt={imageFileName ?? editingCategory.name}
                    className="h-20 w-16 object-cover"
                  />
                  <div className="min-w-0 space-y-1 text-sm">
                    <p className="truncate">
                      {imageFileName ?? "Current category image"}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {isImageUploading
                        ? `Uploading ${imageUploadProgress}%`
                        : imageUrl
                          ? "Ready to save"
                          : "No image selected"}
                    </p>
                  </div>
                </div>
              ) : null}
            </label>
          </div>

          <button
            disabled={isPending || isImageUploading}
            className="bg-primary px-4 py-3 text-sm text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
          >
            {isPending ? "Saving..." : "Save Category"}
          </button>
          <FormError error={error} />
        </form>
      ) : null}
    </div>
  )
}
