import Link from "next/link"
import { ArrowRight } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { defaultStoreSlug, storefrontPath } from "@/lib/config"
import { getStoreDisplayName } from "@/lib/store-branding"

interface FooterProps {
  storeSlug?: string
  storeName?: string | null
}

export function Footer({ storeSlug, storeName }: FooterProps) {
  const resolvedStoreSlug = storeSlug ?? defaultStoreSlug
  const displayName = getStoreDisplayName(storeName, resolvedStoreSlug)
  const homeHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug) : "/"
  const productsHref = resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/products") : "/products"

  const footerLinks = {
    shop: [
      { name: "All Products", href: productsHref },
      { name: "Brands", href: resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/products") : "/brands" },
      { name: "Categories", href: resolvedStoreSlug ? storefrontPath(resolvedStoreSlug, "/products") : "/categories" },
    ],
    support: [
      { name: "Contact", href: "/contact" },
      { name: "Shipping Info", href: "/shipping-policy" },
      { name: "Return Policy", href: "/return-policy" },
    ],
    company: [
      { name: "About Us", href: "/about" },
      { name: "Privacy Policy", href: "/privacy-policy" },
      { name: "Terms", href: "/terms" },
    ],
  }

  return (
    <footer className="border-t border-border bg-foreground text-background">
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8 lg:py-32">
        <div className="grid grid-cols-1 gap-16 lg:grid-cols-12 lg:gap-8">
          <div className="lg:col-span-5">
            <Link href={homeHref} className="inline-block">
              <span className="block max-w-full break-words font-serif text-2xl font-light tracking-[0.28em] text-background sm:text-3xl sm:tracking-[0.4em]">
                {displayName}
              </span>
            </Link>
            <p className="mt-8 max-w-sm text-sm leading-relaxed text-background/60">
              Timeless designs, explicit backend-driven commerce flows, and a storefront
              that stays aligned with your real catalog.
            </p>

            <div className="mt-12">
              <p className="text-[10px] font-normal uppercase tracking-[0.3em] text-background/80">
                Newsletter
              </p>
              <p className="mt-3 text-sm text-background/50">
                Subscription consent should be handled through the account preference flows.
              </p>
              <form className="mt-6 flex flex-col gap-3 sm:flex-row">
                <Input
                  type="email"
                  placeholder="Your email address"
                  className="h-14 flex-1 border-background/20 bg-transparent text-sm text-background placeholder:text-background/40 focus:border-background"
                />
                <Button
                  type="submit"
                  className="h-14 w-full bg-background text-foreground hover:bg-background/90 sm:w-14 sm:px-0"
                >
                  <ArrowRight className="h-5 w-5" strokeWidth={1} />
                  <span className="sr-only">Subscribe</span>
                </Button>
              </form>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-10 sm:grid-cols-3 lg:col-span-7 lg:gap-8">
            {Object.entries(footerLinks).map(([section, links]) => (
              <div key={section}>
                <h3 className="text-[10px] font-normal uppercase tracking-[0.3em] text-background/80">
                  {section}
                </h3>
                <ul className="mt-8 space-y-5">
                  {links.map((link) => (
                    <li key={link.name}>
                      <Link
                        href={link.href}
                        className="text-sm text-background/50 transition-colors hover:text-background"
                      >
                        {link.name}
                      </Link>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>

        <div className="mt-16 flex flex-col items-start justify-between gap-6 border-t border-background/10 pt-10 md:flex-row md:items-center">
          <p className="text-xs leading-relaxed text-background/40">
            {new Date().getFullYear()} {displayName}. All rights reserved.
          </p>
          <div className="flex flex-wrap gap-x-6 gap-y-3 sm:gap-10">
            <Link
              href="/privacy-policy"
              className="text-xs text-background/40 transition-colors hover:text-background"
            >
              Privacy
            </Link>
            <Link
              href="/terms"
              className="text-xs text-background/40 transition-colors hover:text-background"
            >
              Terms
            </Link>
            <Link
              href="/contact"
              className="text-xs text-background/40 transition-colors hover:text-background"
            >
              Contact
            </Link>
          </div>
        </div>
      </div>
    </footer>
  )
}
