import { Suspense } from "react"
import { AuthCallbackContent } from "@/components/auth/auth-callback-content"

export default function AuthCallbackPage() {
  return (
    <Suspense fallback={null}>
      <AuthCallbackContent />
    </Suspense>
  )
}
