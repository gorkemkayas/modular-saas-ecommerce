import { redirect } from "next/navigation"
import { defaultStoreSlug } from "@/lib/config"

export default function LegacyCheckoutPage() {
  if (defaultStoreSlug) {
    redirect(`/${defaultStoreSlug}/checkout`)
  }

  redirect("/")
}
