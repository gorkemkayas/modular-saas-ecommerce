import { redirect } from "next/navigation"
import { defaultStoreSlug } from "@/lib/config"

export default function LegacyProductsPage() {
  if (defaultStoreSlug) {
    redirect(`/${defaultStoreSlug}/products`)
  }

  redirect("/")
}
