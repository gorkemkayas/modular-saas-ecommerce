import Link from "next/link"
import { MessageSquareMore } from "lucide-react"

export function LandingFeedbackButton() {
  return (
    <Link
      href="/contact?source=homepage-feedback"
      className="fixed bottom-5 right-5 z-40 inline-flex items-center gap-2 rounded-full border border-border bg-background/92 px-4 py-3 text-xs uppercase tracking-[0.2em] text-foreground shadow-lg backdrop-blur-sm transition-colors hover:bg-secondary sm:bottom-6 sm:right-6"
      aria-label="Send feedback"
    >
      <MessageSquareMore className="h-4 w-4" strokeWidth={1.5} />
      <span>Feedback</span>
    </Link>
  )
}
