import { redirect } from "next/navigation"
import { AccountLayoutShell } from "@/components/account/account-layout-shell"
import { getAuthSession } from "@/lib/api/auth"
import { withQuery } from "@/lib/config"

export default async function AccountLayout({
  children,
}: {
  children: React.ReactNode
}) {
  const session = await getAuthSession()

  if (!session.isAuthenticated) {
    redirect(withQuery("/auth/login", { next: "/account" }))
  }

  return (
    <AccountLayoutShell>{children}</AccountLayoutShell>
  )
}
