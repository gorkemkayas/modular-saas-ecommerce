import type { Metadata } from "next"

import { ContactPageContent } from "@/components/contact/contact-page-content"
import { listContactFeedbacks } from "@/lib/api/feedback"

export const metadata: Metadata = {
  title: "Contact | KAYAS",
  description:
    "Share feedback, ask questions, and reach the KAYAS team through our contact page.",
}

export default async function ContactPage({
  searchParams,
}: {
  searchParams: Promise<{ source?: string | string[] | undefined }>
}) {
  let feedbacks = []

  try {
    feedbacks = await listContactFeedbacks()
  } catch {
    feedbacks = []
  }

  const resolvedSearchParams = await searchParams
  const source = Array.isArray(resolvedSearchParams.source)
    ? resolvedSearchParams.source[0]
    : resolvedSearchParams.source

  return <ContactPageContent feedbacks={feedbacks} source={source} />
}
