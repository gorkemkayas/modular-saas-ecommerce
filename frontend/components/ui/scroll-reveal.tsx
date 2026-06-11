"use client"

import type { HTMLAttributes, ReactNode } from "react"
import { useEffect, useRef, useState } from "react"

import { cn } from "@/lib/utils"

interface ScrollRevealProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
  delay?: number
  direction?: "up" | "left" | "right" | "none"
}

const hiddenDirectionClasses = {
  up: "translate-y-8",
  left: "-translate-x-8",
  right: "translate-x-8",
  none: "",
}

export function ScrollReveal({
  children,
  className,
  delay = 0,
  direction = "up",
  style,
  ...props
}: ScrollRevealProps) {
  const ref = useRef<HTMLDivElement | null>(null)
  const [isVisible, setIsVisible] = useState(false)

  useEffect(() => {
    const node = ref.current

    if (!node) {
      return
    }

    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      setIsVisible(true)
      return
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry?.isIntersecting) {
          setIsVisible(true)
          observer.unobserve(entry.target)
        }
      },
      {
        rootMargin: "0px 0px -8% 0px",
        threshold: 0.18,
      },
    )

    observer.observe(node)

    return () => observer.disconnect()
  }, [])

  return (
    <div
      ref={ref}
      className={cn(
        "transition-all duration-700 ease-out will-change-transform",
        isVisible
          ? "translate-x-0 translate-y-0 opacity-100"
          : `${hiddenDirectionClasses[direction]} opacity-0`,
        className,
      )}
      style={{ ...style, transitionDelay: `${delay}ms` }}
      {...props}
    >
      {children}
    </div>
  )
}
