export function AdminErrorState({
  title = "Unable to load this section",
  message,
}: {
  title?: string
  message: string
}) {
  return (
    <div className="border border-destructive/30 bg-destructive/5 p-6">
      <h2 className="text-lg font-light tracking-wide">{title}</h2>
      <p className="mt-2 text-sm text-muted-foreground">{message}</p>
    </div>
  )
}
