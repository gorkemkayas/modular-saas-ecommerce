import { redirect } from "next/navigation"
import { defaultStoreSlug } from "@/lib/config"

export default function LegacyCartPage() {
  if (defaultStoreSlug) {
    redirect(`/${defaultStoreSlug}/cart`)
  }

  redirect("/")
}
