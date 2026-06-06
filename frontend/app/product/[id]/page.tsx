import { redirect } from "next/navigation"
import { defaultStoreSlug } from "@/lib/config"

export default async function LegacyProductPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = await params

  if (defaultStoreSlug) {
    redirect(`/${defaultStoreSlug}/products/${id}`)
  }

  redirect("/")
}
