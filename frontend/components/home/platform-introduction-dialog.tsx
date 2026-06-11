"use client"

import { useEffect, useState } from "react"

import { Button } from "@/components/ui/button"
import { toast } from "@/hooks/use-toast"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"

const dialogStorageKey = "kayas-platform-introduction-dialog-dismissed-v1"

export function PlatformIntroductionDialog() {
  const [open, setOpen] = useState(false)

  useEffect(() => {
    const hasSeenDialog = window.localStorage.getItem(dialogStorageKey)

    if (!hasSeenDialog) {
      setOpen(true)
    }
  }, [])

  const handleOpenChange = (nextOpen: boolean) => {
    setOpen(nextOpen)

    if (!nextOpen) {
      window.localStorage.setItem(dialogStorageKey, "true")
      toast({
        title: "Your feedback matters to us",
        description:
          "You can share your thoughts using the Feedback button on the homepage.",
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="max-w-2xl border-border bg-background p-0 sm:max-w-2xl"
        showCloseButton={false}
      >
        <div className="border-b border-border bg-secondary/35 px-6 py-5 sm:px-8">
          <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
            Platform Notice
          </p>
          <DialogHeader className="mt-3 text-left">
            <DialogTitle className="font-serif text-3xl font-light tracking-wide">
              A Quick Note
            </DialogTitle>
            <DialogDescription className="sr-only">
              A short note about the platform's current development stage.
            </DialogDescription>
          </DialogHeader>
        </div>

        <div className="space-y-5 px-6 py-6 sm:px-8">
          <p className="text-sm leading-7 text-muted-foreground">
            Welcome. This platform is currently an MVP that continues to evolve.
            Our goal is to build a solid, sustainable foundation that will grow
            stronger over time. Because of that, some areas may still be at a
            foundational stage, and certain features may continue to be refined
            as development progresses.
          </p>
          <p className="text-sm leading-7 text-muted-foreground">
            This live version has been released both to shape the core product
            structure and to learn from real usage in order to build a better
            system. In the coming period, we aim to mature both the user
            experience and the technical foundation step by step.
          </p>
          <p className="text-sm leading-7 text-muted-foreground">
            Having you here and seeing you experience the platform is genuinely
            valuable to us.
          </p>
        </div>

        <DialogFooter className="border-t border-border px-6 py-5 sm:px-8">
          <Button
            type="button"
            onClick={() => handleOpenChange(false)}
            className="h-11 px-6 text-xs uppercase tracking-[0.2em] sm:min-w-36"
          >
            Continue
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
