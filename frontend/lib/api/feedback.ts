import { fetchJson, postJson } from "@/lib/api/client"

export interface ContactFeedbackDto {
  id: string
  fullName: string
  subject: string
  message: string
  source: string | null
  createdAtUtc: string
}

export interface SubmitContactFeedbackRequest {
  fullName: string
  email: string
  subject: string
  message: string
  source?: string | null
}

export interface SubmitContactFeedbackResponse {
  feedbackId: string
}

export async function submitContactFeedback(
  request: SubmitContactFeedbackRequest,
): Promise<SubmitContactFeedbackResponse> {
  return postJson<SubmitContactFeedbackResponse, SubmitContactFeedbackRequest>(
    "/api/contact-feedback",
    request,
  )
}

export async function listContactFeedbacks(): Promise<ContactFeedbackDto[]> {
  return fetchJson<ContactFeedbackDto[]>("/api/contact-feedback")
}
