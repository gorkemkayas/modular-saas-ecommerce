import { redirect } from "next/navigation"
import { AccountLayoutShell } from "@/components/account/account-layout-shell"
import { getAuthSession, isSessionForStore } from "@/lib/api/auth"
import { storefrontPath, withQuery } from "@/lib/config"

export default async function StoreAccountLayout({
  children,
  params,
}: {
  children: React.ReactNode
  params: Promise<{ storeSlug: string }>
}) {
  const { storeSlug } = await params
  const session = await getAuthSession()

  if (!session.isAuthenticated) {
    redirect(
      withQuery(storefrontPath(storeSlug, "/login"), {
        next: storefrontPath(storeSlug, "/account"),
      }),
    )
  }

  if (!isSessionForStore(session, storeSlug)) {
    redirect(
      withQuery(storefrontPath(storeSlug, "/login"), {
        next: storefrontPath(storeSlug, "/account"),
      }),
    )
  }

  return <AccountLayoutShell>{children}</AccountLayoutShell>
}
