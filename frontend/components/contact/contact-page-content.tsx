"use client"

import { FormEvent, useState } from "react"
import Link from "next/link"
import {
  ArrowRight,
  Clock3,
  Mail,
  MapPin,
  MessageSquareHeart,
  Phone,
  Send,
  Sparkles,
} from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/error-message"
import { submitContactFeedback } from "@/lib/api/feedback"
import { cn } from "@/lib/utils"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import type { ContactFeedbackDto } from "@/lib/api/feedback"

type FormState = {
  fullName: string
  email: string
  subject: string
  message: string
}

const defaultFeedbackSubject = "Homepage feedback"

const contactMethods = [
  {
    icon: Mail,
    title: "Email",
    primary: "contact@kayas.dev",
    secondary: "Best for product questions and detailed feedback",
  },
  {
    icon: Phone,
    title: "Phone",
    primary: "+90 543 872 61 77",
    secondary: "Monday to Friday, 9:00 AM - 6:00 PM TRT",
  },
  {
    icon: MapPin,
    title: "Address",
    primary: "Cumhuriyet Neighborhood, 121st Street, No: 8",
    secondary: "Biga, Canakkale, Turkey",
  },
  {
    icon: Clock3,
    title: "Response Time",
    primary: "Usually within 1-2 business days",
    secondary: "We review every message carefully",
  },
]

const supportTopics = [
  "Product feedback and improvement ideas",
  "Questions about setup, onboarding, or roadmap",
  "Partnership, collaboration, and business inquiries",
]

export function ContactPageContent({
  feedbacks,
  source,
}: {
  feedbacks: ContactFeedbackDto[]
  source?: string | null
}) {
  const fromHomepageFeedback = source === "homepage-feedback"

  const [form, setForm] = useState<FormState>({
    fullName: "",
    email: "",
    subject: fromHomepageFeedback ? defaultFeedbackSubject : "",
    message: "",
  })
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const formSource = source?.trim() || "contact-page"

  const updateField = (field: keyof FormState, value: string) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage(null)
    setSuccessMessage(null)
    setIsSubmitting(true)

    try {
      await submitContactFeedback({
        fullName: form.fullName,
        email: form.email,
        subject: form.subject,
        message: form.message,
        source: formSource,
      })

      setSuccessMessage(
        "Your message has been received successfully. Thank you for helping us improve KAYAS.",
      )
      setForm({
        fullName: "",
        email: "",
        subject: fromHomepageFeedback ? defaultFeedbackSubject : "",
        message: "",
      })
    } catch (error) {
      setErrorMessage(
        getApiErrorMessage(
          error,
          "Your message could not be sent right now. Please try again shortly.",
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="min-h-screen bg-background text-foreground">
      <section className="border-b border-border bg-foreground text-background">
        <div className="mx-auto grid max-w-7xl gap-12 px-4 py-16 sm:px-6 lg:grid-cols-[1.05fr_0.95fr] lg:px-8 lg:py-24">
          <div className="max-w-3xl">
            <p className="inline-flex items-center gap-2 text-xs uppercase tracking-[0.28em] text-background/62">
              <MessageSquareHeart className="h-4 w-4" strokeWidth={1.5} />
              Contact and Feedback
            </p>
            <h1 className="mt-6 font-serif text-4xl font-light tracking-wide sm:text-5xl lg:text-6xl">
              Let&apos;s keep improving the platform together.
            </h1>
            <p className="mt-6 max-w-2xl text-base leading-8 text-background/72">
              Whether you want to share product feedback, report friction in the
              experience, or ask a business question, we&apos;d be glad to hear
              from you. Every message helps shape the next iteration of KAYAS.
            </p>

            <div className="mt-10 grid gap-4 sm:grid-cols-3">
              {supportTopics.map((topic) => (
                <div
                  key={topic}
                  className="border border-background/15 bg-background/5 p-4 text-sm leading-6 text-background/74 backdrop-blur-sm"
                >
                  {topic}
                </div>
              ))}
            </div>
          </div>

          <div className="border border-background/12 bg-background/6 p-6 backdrop-blur-sm sm:p-8">
            <p className="text-xs uppercase tracking-[0.24em] text-background/58">
              Preferred contact
            </p>
            <div className="mt-5 space-y-5">
              {contactMethods.map((method) => {
                const Icon = method.icon

                return (
                  <div
                    key={method.title}
                    className="flex gap-4 border-b border-background/10 pb-5 last:border-b-0 last:pb-0"
                  >
                    <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full border border-background/18 bg-background/7">
                      <Icon className="h-5 w-5" strokeWidth={1.5} />
                    </div>
                    <div>
                      <p className="text-sm uppercase tracking-[0.2em] text-background/52">
                        {method.title}
                      </p>
                      <p className="mt-2 text-base text-background">
                        {method.primary}
                      </p>
                      <p className="mt-1 text-sm leading-6 text-background/64">
                        {method.secondary}
                      </p>
                    </div>
                  </div>
                )
              })}
            </div>
          </div>
        </div>
      </section>

      <section className="px-4 py-16 sm:px-6 lg:px-8 lg:py-20">
        <div className="mx-auto grid max-w-7xl gap-10 lg:grid-cols-[0.78fr_1.22fr]">
          <div className="space-y-6">
            <div className="border border-border bg-secondary/35 p-6">
              <p className="inline-flex items-center gap-2 text-xs uppercase tracking-[0.24em] text-muted-foreground">
                <Sparkles className="h-4 w-4" strokeWidth={1.5} />
                Feedback matters
              </p>
              <h2 className="mt-4 font-serif text-3xl font-light tracking-wide">
                What happens after you send a message?
              </h2>
              <div className="mt-6 space-y-4 text-sm leading-7 text-muted-foreground">
                <p>
                  We review every submission and use recurring themes to improve
                  product direction, usability, and technical priorities.
                </p>
                <p>
                  If your note requires a direct follow-up, we&apos;ll return to
                  you through the email address you provide.
                </p>
              </div>
            </div>

            <div className="border border-border bg-background p-6">
              <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                Need another route?
              </p>
              <p className="mt-4 text-sm leading-7 text-muted-foreground">
                If your request is more about policies, support details, or
                general guidance, you can also review our help content first.
              </p>
              <Link
                href="/help"
                className="mt-5 inline-flex items-center gap-2 text-sm underline underline-offset-4"
              >
                Visit Help Center
                <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
              </Link>
            </div>
          </div>

          <div className="border border-border bg-background p-6 sm:p-8">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <p className="text-xs uppercase tracking-[0.24em] text-muted-foreground">
                  Send a message
                </p>
                <h2 className="mt-3 font-serif text-2xl font-light tracking-wide sm:text-3xl">
                  Tell us what&apos;s on your mind.
                </h2>
              </div>
              {fromHomepageFeedback ? (
                <div className="border border-border bg-secondary/60 px-4 py-3 text-xs uppercase tracking-[0.18em] text-muted-foreground">
                  Feedback shortcut
                </div>
              ) : null}
            </div>

            {fromHomepageFeedback ? (
              <div className="mt-6 border border-border bg-secondary/35 px-4 py-4 text-sm leading-7 text-muted-foreground">
                You came here from the homepage feedback button. If you&apos;re
                sharing an idea, issue, or first impression, this form will save
                it directly for our review.
              </div>
            ) : null}

            <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
              {successMessage ? (
                <Alert className="border-emerald-200 bg-emerald-50 text-emerald-900">
                  <AlertTitle>Message received</AlertTitle>
                  <AlertDescription>{successMessage}</AlertDescription>
                </Alert>
              ) : null}

              {errorMessage ? (
                <Alert variant="destructive">
                  <AlertTitle>Something went wrong</AlertTitle>
                  <AlertDescription>{errorMessage}</AlertDescription>
                </Alert>
              ) : null}

              <div className="grid gap-5 md:grid-cols-2">
                <div>
                  <label
                    htmlFor="fullName"
                    className="mb-2 block text-sm font-medium"
                  >
                    Full name
                  </label>
                  <Input
                    id="fullName"
                    value={form.fullName}
                    onChange={(event) =>
                      updateField("fullName", event.target.value)
                    }
                    placeholder="Your name"
                    className="h-12 rounded-none"
                    maxLength={200}
                    required
                  />
                </div>

                <div>
                  <label
                    htmlFor="email"
                    className="mb-2 block text-sm font-medium"
                  >
                    Email address
                  </label>
                  <Input
                    id="email"
                    type="email"
                    value={form.email}
                    onChange={(event) =>
                      updateField("email", event.target.value)
                    }
                    placeholder="you@example.com"
                    className="h-12 rounded-none"
                    maxLength={320}
                    required
                  />
                </div>
              </div>

              <div>
                <label
                  htmlFor="subject"
                  className="mb-2 block text-sm font-medium"
                >
                  Subject
                </label>
                <Input
                  id="subject"
                  value={form.subject}
                  onChange={(event) =>
                    updateField("subject", event.target.value)
                  }
                  placeholder="How can we help?"
                  className="h-12 rounded-none"
                  maxLength={200}
                  required
                />
              </div>

              <div>
                <label
                  htmlFor="message"
                  className="mb-2 block text-sm font-medium"
                >
                  Message
                </label>
                <Textarea
                  id="message"
                  value={form.message}
                  onChange={(event) =>
                    updateField("message", event.target.value)
                  }
                  placeholder="Share your thoughts, feedback, or request here."
                  className="min-h-40 rounded-none"
                  maxLength={4000}
                  required
                />
              </div>

              <div className="flex flex-col gap-4 border-t border-border pt-6 sm:flex-row sm:items-center sm:justify-between">
                <p className="max-w-xl text-sm leading-7 text-muted-foreground">
                  By sending this form, you allow us to store your message so we
                  can review it and follow up when needed.
                </p>
                <Button
                  type="submit"
                  disabled={isSubmitting}
                  className="h-12 min-w-44 rounded-none text-xs uppercase tracking-[0.22em]"
                >
                  {isSubmitting ? "Sending..." : "Send Message"}
                  <Send className="h-4 w-4" strokeWidth={1.5} />
                </Button>
              </div>
            </form>
          </div>
        </div>
      </section>

      <section className="border-t border-border bg-[linear-gradient(180deg,_rgba(255,255,255,0.98)_0%,_rgba(250,249,246,0.98)_100%)] px-4 py-16 text-foreground sm:px-6 lg:px-8 lg:py-24">
        <div className="mx-auto max-w-7xl">
          <div className="mb-12 grid gap-10 lg:grid-cols-[0.9fr_1.1fr] lg:items-end">
            <div>
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                Feedback Wall
              </p>
              <h2 className="mt-4 font-serif text-3xl font-light tracking-[0.02em] sm:text-4xl lg:text-5xl">
                Shared openly, so the product journey stays visible.
              </h2>
              <p className="mt-5 max-w-2xl text-sm leading-8 text-muted-foreground">
                Every message below was submitted through the contact page and
                is shown here to make the platform feel more transparent,
                human, and accountable as it evolves.
              </p>
            </div>

            <div className="grid gap-4 sm:grid-cols-3">
              <div className="border border-border/80 bg-background/90 p-5">
                <p className="font-serif text-4xl font-light tracking-wide">
                  {feedbacks.length}
                </p>
                <p className="mt-2 text-xs uppercase tracking-[0.22em] text-muted-foreground">
                  Feedback entries
                </p>
              </div>
              <div className="border border-border/80 bg-background/90 p-5">
                <p className="font-serif text-4xl font-light tracking-wide">
                  Live
                </p>
                <p className="mt-2 text-xs uppercase tracking-[0.22em] text-muted-foreground">
                  Transparency view
                </p>
              </div>
              <div className="border border-border/80 bg-background/90 p-5">
                <p className="font-serif text-4xl font-light tracking-wide">
                  Private
                </p>
                <p className="mt-2 text-xs uppercase tracking-[0.22em] text-muted-foreground">
                  Email details
                </p>
              </div>
            </div>
          </div>

          {feedbacks.length === 0 ? (
            <div className="border border-border bg-background px-6 py-8 text-sm text-muted-foreground shadow-sm">
              No feedback has been shared yet.
            </div>
          ) : (
            <div className="border border-border/80 bg-background/95">
              {feedbacks.map((feedback, index) => (
                <article
                  key={feedback.id}
                  className={cn(
                    "group relative grid gap-6 border-b border-border/70 px-6 py-7 transition-colors hover:bg-black/[0.015] sm:px-8 lg:grid-cols-[150px_minmax(0,1fr)_170px] lg:gap-10",
                    index === feedbacks.length - 1 && "border-b-0",
                    index === 0 && "bg-black/[0.02]",
                  )}
                >
                  <div className="flex flex-col gap-3">
                    <p className="text-[11px] uppercase tracking-[0.2em] text-muted-foreground">
                      {index === 0 ? "Featured feedback" : "Feedback note"}
                    </p>
                    <p className="font-serif text-xl font-light tracking-wide text-foreground/70">
                      {formatFeedbackDate(feedback.createdAtUtc)}
                    </p>
                  </div>

                  <div className="min-w-0">
                    <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                      <div className="min-w-0">
                        <h3
                          className={cn(
                            "font-serif text-[1.75rem] font-light tracking-[0.01em] text-foreground",
                            index === 0 && "text-[2rem]",
                          )}
                        >
                          {feedback.subject}
                        </h3>
                        <p
                          className={cn(
                            "mt-4 max-w-3xl text-sm leading-8 text-muted-foreground",
                            index === 0 && "text-[15px]",
                          )}
                        >
                          {feedback.message}
                        </p>
                      </div>
                      {feedback.source ? (
                        <span className="shrink-0 border border-border/80 bg-background px-2.5 py-1 text-[10px] uppercase tracking-[0.18em] text-muted-foreground">
                          {formatSource(feedback.source)}
                        </span>
                      ) : null}
                    </div>
                  </div>

                  <div className="flex flex-col gap-3 border-t border-border/60 pt-4 lg:items-end lg:border-t-0 lg:border-l lg:border-border/60 lg:pt-0 lg:pl-6 lg:text-right">
                    <p className="text-[11px] uppercase tracking-[0.2em] text-muted-foreground">
                      Shared by
                    </p>
                    <p className="font-serif text-[1.7rem] font-light tracking-[0.01em] text-foreground">
                      {feedback.fullName}
                    </p>
                    <p className="text-sm leading-7 text-muted-foreground/90">
                      {index === 0
                        ? "A highlighted public note from the live feedback stream."
                        : "Submitted through the KAYAS contact page."}
                    </p>
                  </div>
                </article>
              ))}
            </div>
          )}
        </div>
      </section>
    </main>
  )
}

function formatFeedbackDate(value: string): string {
  return new Intl.DateTimeFormat("en-US", {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(new Date(value))
}

function formatSource(value: string): string {
  switch (value) {
    case "homepage-feedback":
      return "Homepage"
    case "contact-page":
      return "Contact"
    default:
      return value.replace(/[-_]/g, " ")
  }
}
