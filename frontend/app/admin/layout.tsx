import { headers } from "next/headers"
import { redirect } from "next/navigation"
import { getAuthSession } from "@/lib/api/auth"
import { withQuery } from "@/lib/config"

export default async function AdminLayout({
  children,
}: {
  children: React.ReactNode
}) {
  const session = await getAuthSession()
  const pathname = (await headers()).get("x-pathname") || "/admin"

  if (!session.isAuthenticated) {
    redirect(withQuery("/auth/login", { next: pathname }))
  }

  if (!session.canAccessAdmin || !session.storeSlug) {
    redirect("/unauthorized")
  }

  redirect(`/${session.storeSlug}${pathname}`)
}
