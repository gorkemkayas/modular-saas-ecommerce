import { redirect } from "next/navigation"
import { getAdminPath } from "@/lib/admin-path"

export default async function AdminProductEditRedirect({
  params,
}: {
  params: Promise<{ id: string; storeSlug?: string }>
}) {
  const { id, storeSlug } = await params
  redirect(`${getAdminPath(storeSlug, "/products/edit")}?id=${id}`)
}
