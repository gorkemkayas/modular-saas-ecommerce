import Link from "next/link"
import Image from "next/image"
import { ArrowDownRight } from "lucide-react"

import { Button } from "@/components/ui/button"
import { storefrontPath } from "@/lib/config"

interface HeroSectionProps {
  storeSlug: string
  storeName?: string | null
  heroImageUrl?: string | null
  heroMediaType?: string | null
  heroEyebrowText?: string | null
  heroTitle?: string | null
  heroAccentTitle?: string | null
  heroDescription?: string | null
  heroPrimaryButtonText?: string | null
}

const defaultHero = {
  imageUrl:
    "https://images.unsplash.com/photo-1509631179647-0177331693ae?w=1920&h=1280&fit=crop&q=90",
  eyebrowText: "Spring / Summer 2026",
  title: "Timeless",
  accentTitle: "Elegance",
  description:
    "Crafted by masters, timeless pieces. Our collection defined by premium fabrics and sharp lines.",
  primaryButtonText: "Explore Collection",
} as const

export function HeroSection({
  storeSlug,
  storeName,
  heroImageUrl,
  heroMediaType,
  heroEyebrowText,
  heroTitle,
  heroAccentTitle,
  heroDescription,
  heroPrimaryButtonText,
}: HeroSectionProps) {
  const resolvedImageUrl = heroImageUrl?.trim() || defaultHero.imageUrl
  const resolvedEyebrowText =
    heroEyebrowText?.trim() || defaultHero.eyebrowText
  const resolvedTitle = heroTitle?.trim() || defaultHero.title
  const resolvedAccentTitle =
    heroAccentTitle?.trim() || defaultHero.accentTitle
  const resolvedDescription =
    heroDescription?.trim() || defaultHero.description
  const resolvedPrimaryButtonText =
    heroPrimaryButtonText?.trim() || defaultHero.primaryButtonText
  const isVideoHero = heroMediaType?.toLowerCase() === "video" && !!heroImageUrl?.trim()
  const heroAltText = storeName
    ? `${storeName} storefront hero`
    : "Premium fashion"

  return (
    <section className="relative h-screen overflow-hidden bg-background">
      <div className="absolute inset-0">
        {isVideoHero ? (
          <video
            src={resolvedImageUrl}
            autoPlay
            muted
            loop
            playsInline
            className="h-full w-full object-cover object-center"
          />
        ) : (
          <Image
            src={resolvedImageUrl}
            alt={heroAltText}
            fill
            className="object-cover object-center"
            priority
            quality={95}
          />
        )}
        <div className="absolute inset-0 bg-black/30" />
      </div>

      <div className="relative z-10 flex h-full flex-col justify-between px-6 py-32 lg:px-16">
        <div className="animate-fade-up opacity-0">
          <p className="text-[10px] font-normal uppercase tracking-[0.4em] text-white/70">
            {resolvedEyebrowText}
          </p>
        </div>

        <div className="flex flex-col items-start">
          <h1 className="animate-fade-up opacity-0 animation-delay-200 font-serif text-[clamp(3rem,12vw,10rem)] font-light leading-[0.9] tracking-[-0.02em] text-white">
            {resolvedTitle}
            <br />
            <span className="italic">{resolvedAccentTitle}</span>
          </h1>
          <p className="mt-8 max-w-lg animate-fade-up opacity-0 animation-delay-400 text-sm font-light leading-relaxed tracking-wide text-white/80 lg:text-base">
            {resolvedDescription}
          </p>
          <div className="mt-10 animate-fade-up opacity-0 animation-delay-600">
            <Button
              asChild
              size="lg"
              className="group h-14 border border-white/30 bg-white/10 px-10 text-[11px] uppercase tracking-[0.3em] text-white backdrop-blur-sm transition-all duration-500 hover:bg-white hover:text-black"
            >
              <Link href={storefrontPath(storeSlug, "/products")}>
                {resolvedPrimaryButtonText}
                <ArrowDownRight className="ml-3 h-4 w-4 transition-transform duration-300 group-hover:translate-x-1 group-hover:translate-y-1" />
              </Link>
            </Button>
          </div>
        </div>

        <div className="flex items-end justify-between">
          <div className="animate-fade-in opacity-0 animation-delay-600">
            <span className="text-[10px] uppercase tracking-[0.4em] text-white/50">
              Premium Quality
            </span>
          </div>

          <div className="flex flex-col items-center gap-3 animate-fade-in opacity-0 animation-delay-600">
            <span className="text-[9px] uppercase tracking-[0.4em] text-white/50 [writing-mode:vertical-rl]">
              Scroll Down
            </span>
            <div className="h-16 w-px bg-gradient-to-b from-white/50 to-transparent" />
          </div>
        </div>
      </div>
    </section>
  )
}
