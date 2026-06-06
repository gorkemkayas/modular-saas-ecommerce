import { headers } from "next/headers"
import { redirect } from "next/navigation"
import { AdminLayoutShell } from "@/components/admin/admin-layout-shell"
import { getStoreSettings } from "@/lib/api/admin"
import { getAuthSession } from "@/lib/api/auth"
import { storefrontPath, withQuery } from "@/lib/config"

export default async function StoreAdminLayout({
  children,
  params,
}: {
  children: React.ReactNode
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const pathname = (await headers()).get("x-pathname") || storefrontPath(storeSlug, "/admin")
  const session = await getAuthSession()

  if (!session.isAuthenticated) {
    redirect(
      withQuery("/auth/login", {
        intent: "admin",
        storeSlug,
        next: pathname,
      }),
    )
  }

  if (
    !session.canAccessAdmin ||
    !session.storeSlug ||
    session.storeSlug.toLowerCase() != storeSlug.toLowerCase()
  ) {
    redirect(storefrontPath(storeSlug, "/unauthorized"))
  }

  const store = await getStoreSettings()

  return <AdminLayoutShell storeName={store.name}>{children}</AdminLayoutShell>
}
