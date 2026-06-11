import type { Metadata } from "next"
import type { ReactNode } from "react"
import Link from "next/link"
import {
  ArrowRight,
  BarChart3,
  Boxes,
  Check,
  ClipboardCheck,
  CreditCard,
  Gauge,
  LockKeyhole,
  Package,
  Route,
  ServerCog,
  ShieldCheck,
  Store,
  Truck,
  Users,
} from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/error-message"
import {
  getPublishedStorefrontStores,
  type StorefrontStoreSummaryDto,
} from "@/lib/api/storefront"
import {
  formatSubscriptionLimit,
  getPublicPlans,
  getSubscriptionFeatureLabel,
  getSubscriptionQuotaLabel,
  subscriptionFeatureKeys,
  subscriptionQuotaKeys,
  type SubscriptionPlanDto,
  type SubscriptionQuotaKey,
} from "@/lib/api/subscription"
import { storefrontPath, withQuery } from "@/lib/config"
import { LandingFeedbackButton } from "@/components/home/landing-feedback-button"
import { PlatformIntroductionDialog } from "@/components/home/platform-introduction-dialog"
import { ScrollReveal } from "@/components/ui/scroll-reveal"

export const dynamic = "force-dynamic"

export const metadata: Metadata = {
  title: "KAYAS | Commerce Platform",
  description:
    "A premium multi-tenant commerce platform for launching, managing, and scaling modern online stores.",
}

const heroImageUrl =
  "/images/platform/store-setup-hero.png"

const packageQuotaKeys = [
  subscriptionQuotaKeys.catalogProducts,
  subscriptionQuotaKeys.catalogMediaPerProduct,
  subscriptionQuotaKeys.shippingCarriers,
] satisfies SubscriptionQuotaKey[]

const comparisonQuotaKeys = [
  subscriptionQuotaKeys.catalogProducts,
  subscriptionQuotaKeys.catalogCategories,
  subscriptionQuotaKeys.catalogMediaPerProduct,
  subscriptionQuotaKeys.pricingPriceLists,
  subscriptionQuotaKeys.shippingCarriers,
] satisfies SubscriptionQuotaKey[]

const comparisonFeatureKeys = [
  subscriptionFeatureKeys.variantProducts,
  subscriptionFeatureKeys.storefrontVideoHero,
] as const

const platformModules = [
  {
    icon: <Package className="h-5 w-5" strokeWidth={1.5} />,
    title: "Catalog",
    description:
      "Organize products, categories, brands, media, and variants without losing operational control.",
  },
  {
    icon: <CreditCard className="h-5 w-5" strokeWidth={1.5} />,
    title: "Pricing",
    description:
      "Manage price lists and commercial flexibility as your store moves into higher plan tiers.",
  },
  {
    icon: <Boxes className="h-5 w-5" strokeWidth={1.5} />,
    title: "Inventory",
    description:
      "Keep stock visibility close to the products your customers browse and buy.",
  },
  {
    icon: <Truck className="h-5 w-5" strokeWidth={1.5} />,
    title: "Shipment",
    description:
      "Connect more shipping carriers as the store grows and fulfillment becomes more complex.",
  },
  {
    icon: <Store className="h-5 w-5" strokeWidth={1.5} />,
    title: "Storefront",
    description:
      "Shape a polished customer-facing store with plan-based presentation capabilities.",
  },
  {
    icon: <ShieldCheck className="h-5 w-5" strokeWidth={1.5} />,
    title: "Subscription",
    description:
      "Turn every plan into clear product limits, premium features, and predictable growth paths.",
  },
]

const customerOutcomes = [
  {
    icon: <Store className="h-5 w-5" strokeWidth={1.5} />,
    title: "Launch with confidence",
    description:
      "Start with the essentials: a clean storefront, a manageable catalog, and the core tools needed to sell online.",
  },
  {
    icon: <BarChart3 className="h-5 w-5" strokeWidth={1.5} />,
    title: "Scale when demand grows",
    description:
      "Move into larger catalogs, more media, advanced pricing, and richer storefront options when the business needs them.",
  },
  {
    icon: <Users className="h-5 w-5" strokeWidth={1.5} />,
    title: "Give teams a clearer workspace",
    description:
      "Store owners can see what is available in their plan before they spend time configuring unavailable capabilities.",
  },
]

const dataMetrics = [
  {
    value: "1",
    label: "Store setup flow",
    description:
      "A store owner chooses a plan first, then continues with business and owner details.",
  },
  {
    value: "5",
    label: "Growth limits",
    description:
      "Product, category, media, price list, and shipping capacity scale with the selected plan.",
  },
  {
    value: "2",
    label: "Premium upgrades",
    description:
      "Variant products and video hero presentation are unlocked for stores that need a richer catalog and brand experience.",
  },
  {
    value: "0",
    label: "Surprise restrictions",
    description:
      "Plan differences are visible before the merchant starts building the store.",
  },
]

const lifecycleSteps = [
  {
    step: "01",
    title: "Choose the right starting point",
    label: "Plan Selection",
    description:
      "The merchant begins by selecting the plan that matches their current catalog size, brand needs, and fulfillment setup.",
    data: "starter | growth | premium",
  },
  {
    step: "02",
    title: "Create the store owner account",
    label: "Registration",
    description:
      "The platform collects the store and owner details, then sends the selected plan with the registration request.",
    data: "store name + owner + plan",
  },
  {
    step: "03",
    title: "Provision the store workspace",
    label: "Store Setup",
    description:
      "The store is prepared with an active subscription so the admin panel can show the right limits and capabilities.",
    data: "tenant subscription",
  },
  {
    step: "04",
    title: "Grow inside clear boundaries",
    label: "Daily Operations",
    description:
      "As products, media, shipping carriers, and pricing rules are added, the platform protects the plan boundaries automatically.",
    data: "usage + limit + feature access",
  },
]

const decisionMatrix = [
  {
    icon: <Route className="h-5 w-5" strokeWidth={1.5} />,
    title: "Guided onboarding",
    signal: "selected plan",
    description:
      "The plan selected on the landing page carries into registration, so the merchant always knows what they are buying into.",
  },
  {
    icon: <Gauge className="h-5 w-5" strokeWidth={1.5} />,
    title: "Capacity control",
    signal: "usage vs. limit",
    description:
      "The platform measures catalog, media, pricing, and shipping usage before allowing the next operation.",
  },
  {
    icon: <LockKeyhole className="h-5 w-5" strokeWidth={1.5} />,
    title: "Premium access",
    signal: "feature availability",
    description:
      "Advanced capabilities stay reserved for plans designed for larger, more brand-heavy stores.",
  },
  {
    icon: <BarChart3 className="h-5 w-5" strokeWidth={1.5} />,
    title: "Upgrade clarity",
    signal: "admin visibility",
    description:
      "The admin experience makes plan limits visible, helping merchants understand when an upgrade makes sense.",
  },
]

const governanceRows = [
  {
    title: "Add more products",
    area: "Catalog",
    check: "Product capacity",
    result: "The catalog grows according to the selected plan.",
  },
  {
    title: "Upload product photos",
    area: "Catalog",
    check: "Media per product",
    result: "Each product keeps a plan-based media allowance.",
  },
  {
    title: "Create variant products",
    area: "Catalog",
    check: "Variant access",
    result: "Advanced product structures are reserved for higher tiers.",
  },
  {
    title: "Use multiple price lists",
    area: "Pricing",
    check: "Price list capacity",
    result: "Campaign and channel pricing can expand with the business.",
  },
  {
    title: "Connect shipping carriers",
    area: "Shipment",
    check: "Carrier capacity",
    result: "Fulfillment options scale as order operations become more advanced.",
  },
  {
    title: "Add video hero content",
    area: "Storefront",
    check: "Brand presentation",
    result: "Premium stores can create a richer first impression.",
  },
]

export default async function RootPage() {
  let plans: SubscriptionPlanDto[] = []
  let partnerStores: StorefrontStoreSummaryDto[] = []
  let planLoadError: string | null = null

  try {
    plans = await getPublicPlans()
  } catch (error) {
    planLoadError = getApiErrorMessage(
      error,
      "Subscription plans could not be loaded.",
    )
  }

  try {
    partnerStores = await getPublishedStorefrontStores(16)
  } catch {
    partnerStores = []
  }

  return (
    <main className="min-h-screen bg-background text-foreground">
      <PlatformIntroductionDialog />
      <LandingFeedbackButton />

      <section
        className="relative min-h-[86vh] bg-cover bg-center"
        style={{ backgroundImage: `url('${heroImageUrl}')` }}
      >
        <div className="absolute inset-0 bg-black/45" />
        <header className="relative z-10 mx-auto flex max-w-7xl flex-col gap-4 px-4 py-6 sm:flex-row sm:items-center sm:justify-between sm:px-6 lg:px-8">
          <Link
            href="/"
            className="font-serif text-xl font-light tracking-[0.22em] text-white sm:text-2xl sm:tracking-[0.3em]"
          >
            KAYAS
          </Link>
          <nav className="flex w-full flex-wrap items-center gap-3 text-sm text-white/75 sm:w-auto sm:gap-5">
            <a
              href="#platform"
              className="hidden transition-colors hover:text-white sm:inline"
            >
              Platform
            </a>
            <a
              href="#plans"
              className="hidden transition-colors hover:text-white sm:inline"
            >
              Plans
            </a>
            <Link
              href="/store-register"
              className="inline-flex min-h-11 items-center border border-white/40 px-4 py-2 text-white transition-colors hover:bg-white hover:text-foreground"
            >
              Create Store
            </Link>
          </nav>
        </header>

        <div className="relative z-10 mx-auto grid max-w-7xl gap-10 px-4 pb-14 pt-10 sm:px-6 sm:pt-16 lg:grid-cols-[1fr_420px] lg:gap-12 lg:px-8 lg:pt-24">
          <div className="max-w-3xl text-white">
            <p className="flex items-center gap-2 text-xs uppercase tracking-[0.28em] text-white/70">
              <Store className="h-4 w-4" strokeWidth={1.5} />
              Commerce Platform for Growing Stores
            </p>
            <h1 className="mt-6 font-serif text-4xl font-light tracking-wide sm:text-5xl lg:text-7xl">
              Launch a store that can grow with your business.
            </h1>
            <p className="mt-6 max-w-2xl text-base leading-relaxed text-white/78">
              KAYAS helps store owners create a polished online store, manage
              daily commerce operations, and upgrade into more advanced
              capabilities when the business is ready.
            </p>
            <div className="mt-10 flex flex-col gap-3 sm:flex-row">
              <Link
                href="/store-register"
                className="inline-flex h-14 items-center justify-center gap-2 bg-white px-8 text-sm uppercase tracking-[0.2em] text-foreground transition-colors hover:bg-white/90"
              >
                Start Your Store
                <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
              </Link>
              <a
                href="#plans"
                className="inline-flex h-14 items-center justify-center border border-white/45 px-8 text-sm uppercase tracking-[0.2em] text-white transition-colors hover:bg-white hover:text-foreground"
              >
                Compare Plans
              </a>
            </div>
          </div>

          <aside className="border border-white/22 bg-black/22 p-4 text-white backdrop-blur-sm sm:p-5">
            <p className="text-xs uppercase tracking-[0.28em] text-white/58">
              Store Growth Snapshot
            </p>
            <div className="mt-5 grid grid-cols-2 gap-3">
              <MetricBlock value="8+" label="Commerce areas" />
              <MetricBlock value="3" label="Plan tiers" />
              <MetricBlock value="5" label="Growth limits" />
              <MetricBlock value="2" label="Premium unlocks" />
            </div>
            <div className="mt-5 border-t border-white/16 pt-5">
              <p className="text-sm leading-relaxed text-white/72">
                Every store starts with a plan. That plan shapes catalog size,
                media capacity, pricing flexibility, shipping options, and
                premium storefront features.
              </p>
            </div>
          </aside>
        </div>
      </section>

      <DataMetricsBand />

      <PartnerStoresSection stores={partnerStores} />

      <section
        id="platform"
        className="border-b border-border bg-background px-6 py-16 lg:px-8"
      >
        <ScrollReveal className="mx-auto grid max-w-7xl gap-10 lg:grid-cols-[0.85fr_1.15fr] lg:items-start">
          <div>
            <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
              Built for Store Owners
            </p>
            <h2 className="mt-4 font-serif text-4xl font-light tracking-wide">
              A focused commerce workspace from first launch to premium growth.
            </h2>
            <p className="mt-5 text-sm leading-relaxed text-muted-foreground">
              KAYAS brings the essential store operations into one experience:
              products, pricing, inventory, shipping, storefront settings, and
              subscription-based growth. The merchant sees a simple workspace;
              the platform keeps each capability aligned with the active plan.
            </p>
          </div>

          <div className="grid gap-5 sm:grid-cols-3">
            {customerOutcomes.map((outcome, index) => (
              <ScrollReveal key={outcome.title} delay={(index + 1) * 80}>
                <ValueBlock {...outcome} />
              </ScrollReveal>
            ))}
          </div>
        </ScrollReveal>
      </section>

      <LifecycleStorySection />

      <section className="border-b border-border bg-secondary/45 px-6 py-16 lg:px-8">
        <div className="mx-auto max-w-7xl">
          <ScrollReveal className="mb-10 grid gap-8 lg:grid-cols-[0.8fr_1.2fr]">
            <div>
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                Commerce Operations
              </p>
              <h2 className="mt-4 font-serif text-3xl font-light tracking-wide">
                One admin experience for the work that keeps a store moving.
              </h2>
            </div>
            <p className="text-sm leading-relaxed text-muted-foreground">
              Merchants do not need to think in modules. They need to add
              products, organize the catalog, manage prices, control stock,
              configure delivery, and keep the storefront presentable. KAYAS
              connects those workflows to the plan they selected.
            </p>
          </ScrollReveal>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {platformModules.map((module, index) => (
              <ScrollReveal key={module.title} delay={index * 70}>
                <ModuleBlock {...module} />
              </ScrollReveal>
            ))}
          </div>
        </div>
      </section>

      <DecisionMatrixSection />

      <GovernanceMatrixSection />

      <section id="plans" className="px-6 py-16 lg:px-8">
        <ScrollReveal className="mx-auto max-w-7xl">
          <div className="mb-10 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
            <div>
              <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
                Plans
              </p>
              <h2 className="mt-4 font-serif text-4xl font-light tracking-wide">
                Choose the plan that fits your next stage.
              </h2>
              <p className="mt-4 max-w-2xl text-sm leading-relaxed text-muted-foreground">
                Start lean, grow into advanced selling tools, and move to premium
                presentation when brand experience becomes a bigger part of your
                sales strategy.
              </p>
            </div>
            <Link
              href="/store-register"
              className="inline-flex items-center gap-2 text-sm underline underline-offset-4"
            >
              Start without preselecting
              <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
            </Link>
          </div>

          {planLoadError ? (
            <div className="border border-border bg-secondary px-4 py-3 text-sm text-muted-foreground">
              {planLoadError}
            </div>
          ) : (
            <>
              <div className="grid gap-4 lg:grid-cols-3">
                {plans.map((plan) => (
                  <Link
                    key={plan.code}
                    href={withQuery("/store-register", { plan: plan.code })}
                    className="group border border-border p-6 transition-colors hover:border-foreground hover:bg-secondary/60"
                  >
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="text-xl font-medium">{plan.name}</p>
                        <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                          {plan.description ?? "A plan for your store stage."}
                        </p>
                      </div>
                      <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md border border-border transition-colors group-hover:border-foreground">
                        <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
                      </span>
                    </div>

                    <div className="mt-6 space-y-3 text-sm">
                      {packageQuotaKeys.map((quotaKey) => (
                        <div
                          key={`${plan.code}-${quotaKey}`}
                          className="flex items-center justify-between gap-4"
                        >
                          <span className="text-muted-foreground">
                            {getSubscriptionQuotaLabel(quotaKey)}
                          </span>
                          <span>{getPlanQuotaValue(plan, quotaKey)}</span>
                        </div>
                      ))}
                    </div>

                    <div className="mt-6 border-t border-border pt-4">
                      {plan.features
                        .filter((feature) => feature.isEnabled)
                        .slice(0, 3)
                        .map((feature) => (
                          <div
                            key={`${plan.code}-${feature.key}`}
                            className="mt-2 flex items-center gap-2 text-sm"
                          >
                            <Check className="h-4 w-4" strokeWidth={1.5} />
                            <span>{feature.description ?? feature.key}</span>
                          </div>
                        ))}
                    </div>
                  </Link>
                ))}
              </div>

              <PlanComparisonTable plans={plans} />
            </>
          )}
        </ScrollReveal>
      </section>

      <LandingFooter />
    </main>
  )
}

function LandingFooter() {
  return (
    <footer className="border-t border-border bg-foreground px-4 py-12 text-background sm:px-6 lg:px-8">
      <div className="mx-auto max-w-7xl">
        <div className="grid gap-10 sm:grid-cols-2 lg:grid-cols-[1.1fr_0.9fr_0.9fr_0.9fr]">
          <div>
            <Link
              href="/"
              className="font-serif text-2xl font-light tracking-[0.3em]"
            >
              KAYAS
            </Link>
            <p className="mt-5 max-w-sm text-sm leading-relaxed text-background/68">
              A commerce platform for merchants who want to launch cleanly,
              operate with control, and scale into premium storefront
              capabilities when the business is ready.
            </p>
          </div>

          <FooterColumn
            title="Platform"
            links={[
              { label: "Modules", href: "#platform" },
              { label: "Growth model", href: "#plans" },
              { label: "Plan comparison", href: "#plans" },
            ]}
          />

          <FooterColumn
            title="Store Setup"
            links={[
              { label: "Create store", href: "/store-register" },
              { label: "Choose a plan", href: "#plans" },
              { label: "Launch journey", href: "#platform" },
            ]}
          />

          <div>
            <p className="text-xs uppercase tracking-[0.24em] text-background/48">
              Start
            </p>
            <p className="mt-4 text-sm leading-relaxed text-background/68">
              Pick a plan, enter store details, and prepare a workspace built
              around the capabilities your merchant needs.
            </p>
            <Link
              href="/store-register"
              className="mt-5 inline-flex items-center gap-2 border border-background/25 px-4 py-3 text-xs uppercase tracking-[0.18em] transition-colors hover:bg-background hover:text-foreground"
            >
              Create Store
              <ArrowRight className="h-4 w-4" strokeWidth={1.5} />
            </Link>
          </div>
        </div>

        <div className="mt-10 flex flex-col gap-3 border-t border-background/12 pt-6 text-xs uppercase tracking-[0.18em] text-background/42 sm:flex-row sm:items-center sm:justify-between">
          <p>2026 KAYAS Commerce Platform</p>
          <p>Built for modular, multi-tenant commerce</p>
        </div>
      </div>
    </footer>
  )
}

function FooterColumn({
  title,
  links,
}: {
  title: string
  links: Array<{ label: string; href: string }>
}) {
  return (
    <div>
      <p className="text-xs uppercase tracking-[0.24em] text-background/48">
        {title}
      </p>
      <div className="mt-4 space-y-3">
        {links.map((link) => (
          <Link
            key={`${title}-${link.label}`}
            href={link.href}
            className="block text-sm text-background/68 transition-colors hover:text-background"
          >
            {link.label}
          </Link>
        ))}
      </div>
    </div>
  )
}

function MetricBlock({ value, label }: { value: string; label: string }) {
  return (
    <div className="border border-white/16 px-4 py-4">
      <p className="font-serif text-3xl font-light tracking-wide">{value}</p>
      <p className="mt-1 text-xs uppercase tracking-[0.18em] text-white/54">
        {label}
      </p>
    </div>
  )
}

function DataMetricsBand() {
  return (
    <section className="border-b border-border bg-foreground px-6 py-10 text-background lg:px-8">
      <div className="mx-auto grid max-w-7xl gap-8 lg:grid-cols-[0.55fr_1.45fr] lg:items-center">
        <ScrollReveal direction="left">
          <p className="text-xs uppercase tracking-[0.28em] text-background/55">
            What Scales With You
          </p>
          <h2 className="mt-3 font-serif text-3xl font-light tracking-wide">
            The plan you choose becomes the operating model for your store.
          </h2>
        </ScrollReveal>

        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          {dataMetrics.map((metric, index) => (
            <ScrollReveal key={metric.label} delay={index * 80}>
              <div className="min-h-40 border border-background/16 p-5">
                <p className="font-serif text-4xl font-light tracking-wide">
                  {metric.value}
                </p>
                <p className="mt-4 text-xs uppercase tracking-[0.2em] text-background/56">
                  {metric.label}
                </p>
                <p className="mt-3 text-sm leading-relaxed text-background/70">
                  {metric.description}
                </p>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}

function PartnerStoresSection({
  stores,
}: {
  stores: StorefrontStoreSummaryDto[]
}) {
  if (stores.length === 0) {
    return null
  }

  const marqueeStores = [...stores, ...stores, ...stores, ...stores]

  return (
    <section className="border-b border-border bg-background py-16">
      <ScrollReveal>
        <div className="mx-auto mb-10 flex max-w-7xl flex-col gap-4 px-6 md:flex-row md:items-end md:justify-between lg:px-8">
          <div>
            <p className="inline-flex items-center gap-3 text-xs uppercase tracking-[0.28em] text-muted-foreground">
              <span className="relative flex h-2.5 w-2.5" aria-hidden="true">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-red-500 opacity-70" />
                <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-red-500 shadow-[0_0_0_3px_rgba(239,68,68,0.12)]" />
              </span>
              Stores on KAYAS
            </p>
            <h2 className="mt-4 font-serif text-3xl font-light tracking-wide">
              A growing collection of live storefronts.
            </h2>
          </div>
          <p className="max-w-lg text-sm leading-relaxed text-muted-foreground md:text-right">
            A refined, live selection of active stores registered in the system.
          </p>
        </div>

        <div className="store-marquee-shell overflow-hidden border-y border-border bg-background py-7">
          <div className="store-marquee-track flex w-max items-center">
            {marqueeStores.map((store, index) => (
              <Link
                key={`${store.tenantId}-${index}`}
                href={storefrontPath(store.slug)}
                className="group flex min-w-[260px] items-center justify-center px-8"
              >
                <span className="h-px w-10 bg-border transition-colors group-hover:bg-foreground" />
                <span className="mx-6 max-w-[220px] truncate text-center font-serif text-2xl font-light tracking-wide text-foreground/78 transition-colors group-hover:text-foreground">
                  {store.name}
                </span>
                <span className="h-px w-10 bg-border transition-colors group-hover:bg-foreground" />
              </Link>
            ))}
          </div>
        </div>
      </ScrollReveal>
    </section>
  )
}

function LifecycleStorySection() {
  return (
    <section className="border-b border-border bg-background px-6 py-20 lg:px-8">
      <div className="mx-auto grid max-w-7xl gap-10 lg:grid-cols-[0.72fr_1.28fr]">
        <ScrollReveal
          direction="left"
          className="self-start lg:sticky lg:top-24"
        >
          <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
            Store Journey
          </p>
          <h2 className="mt-4 font-serif text-4xl font-light tracking-wide">
            From plan selection to a working store, each step is intentional.
          </h2>
          <p className="mt-5 text-sm leading-relaxed text-muted-foreground">
            The merchant journey starts with a business decision, not a blank
            form. KAYAS uses that decision to prepare the right workspace,
            protect plan boundaries, and make upgrades understandable later.
          </p>

          <div className="mt-8 grid gap-3 sm:grid-cols-2 lg:grid-cols-1 xl:grid-cols-2">
            <div className="border border-border p-4">
              <div className="flex items-center gap-3">
                <ServerCog className="h-5 w-5" strokeWidth={1.5} />
                <p className="text-sm font-medium">Setup context</p>
              </div>
              <p className="mt-3 text-xs uppercase tracking-[0.18em] text-muted-foreground">
                store + owner + selected plan
              </p>
            </div>
            <div className="border border-border p-4">
              <div className="flex items-center gap-3">
                <ClipboardCheck className="h-5 w-5" strokeWidth={1.5} />
                <p className="text-sm font-medium">Ongoing guidance</p>
              </div>
              <p className="mt-3 text-xs uppercase tracking-[0.18em] text-muted-foreground">
                usage + limit + upgrade signal
              </p>
            </div>
          </div>
        </ScrollReveal>

        <div className="space-y-5">
          {lifecycleSteps.map((item, index) => (
            <ScrollReveal
              key={item.step}
              delay={index * 90}
              direction="right"
            >
              <div className="grid gap-5 border border-border bg-secondary/35 p-5 sm:grid-cols-[96px_1fr]">
                <div>
                  <p className="font-serif text-5xl font-light tracking-wide text-muted-foreground/45">
                    {item.step}
                  </p>
                  <p className="mt-3 text-xs uppercase tracking-[0.2em] text-muted-foreground">
                    {item.label}
                  </p>
                </div>
                <div>
                  <h3 className="font-serif text-2xl font-light tracking-wide">
                    {item.title}
                  </h3>
                  <p className="mt-3 text-sm leading-relaxed text-muted-foreground">
                    {item.description}
                  </p>
                  <div className="mt-5 border-t border-border pt-4">
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      Platform signal
                    </p>
                    <p className="mt-2 text-sm">{item.data}</p>
                  </div>
                </div>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}

function DecisionMatrixSection() {
  return (
    <section className="border-b border-border bg-background px-6 py-16 lg:px-8">
      <div className="mx-auto max-w-7xl">
        <ScrollReveal className="mb-10 grid gap-8 lg:grid-cols-[0.8fr_1.2fr]">
          <div>
            <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
              Guided Growth
            </p>
            <h2 className="mt-4 font-serif text-3xl font-light tracking-wide">
              Premium feels better when the customer understands what changes.
            </h2>
          </div>
          <p className="text-sm leading-relaxed text-muted-foreground">
            A plan is not just a price point. It defines what the merchant can
            build today, what becomes available as they grow, and where the next
            upgrade creates meaningful value for the business.
          </p>
        </ScrollReveal>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {decisionMatrix.map((item, index) => (
            <ScrollReveal key={item.title} delay={index * 80}>
              <div className="h-full border border-border p-5">
                <div className="flex h-10 w-10 items-center justify-center rounded-md bg-secondary">
                  {item.icon}
                </div>
                <h3 className="mt-5 text-base font-medium">{item.title}</h3>
                <p className="mt-3 text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  {item.signal}
                </p>
                <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
                  {item.description}
                </p>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}

function GovernanceMatrixSection() {
  return (
    <section className="border-b border-border bg-secondary/45 px-6 py-16 lg:px-8">
      <ScrollReveal className="mx-auto max-w-7xl">
        <div className="mb-10 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
          <div>
            <p className="text-xs uppercase tracking-[0.28em] text-muted-foreground">
              Plan Value Map
            </p>
            <h2 className="mt-4 font-serif text-3xl font-light tracking-wide">
              See how each plan difference turns into a practical store benefit.
            </h2>
          </div>
          <p className="max-w-xl text-sm leading-relaxed text-muted-foreground md:text-right">
            These are the everyday moments where a merchant feels the difference
            between starting lean and growing into more advanced operations.
          </p>
        </div>

        <div className="overflow-x-auto border border-border bg-background">
          <table className="w-full min-w-[860px] border-collapse text-sm">
            <thead>
              <tr className="border-b border-border bg-secondary/60">
                <th className="px-4 py-4 text-left text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  Merchant Goal
                </th>
                <th className="px-4 py-4 text-left text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  Area
                </th>
                <th className="px-4 py-4 text-left text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  Plan Rule
                </th>
                <th className="px-4 py-4 text-left text-xs uppercase tracking-[0.2em] text-muted-foreground">
                  Customer Benefit
                </th>
              </tr>
            </thead>
            <tbody>
              {governanceRows.map((row) => (
                <tr key={row.check} className="border-b border-border last:border-b-0">
                  <td className="px-4 py-4 font-medium">{row.title}</td>
                  <td className="px-4 py-4 text-muted-foreground">{row.area}</td>
                  <td className="px-4 py-4">
                    <span className="border border-border bg-secondary px-2 py-1 text-xs">
                      {row.check}
                    </span>
                  </td>
                  <td className="px-4 py-4 text-muted-foreground">
                    {row.result}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </ScrollReveal>
    </section>
  )
}

function ValueBlock({
  icon,
  title,
  description,
}: {
  icon: ReactNode
  title: string
  description: string
}) {
  return (
    <div className="border border-border bg-background p-5">
      <div className="flex h-10 w-10 items-center justify-center rounded-md bg-secondary">
        {icon}
      </div>
      <h3 className="mt-5 text-base font-medium">{title}</h3>
      <p className="mt-3 text-sm leading-relaxed text-muted-foreground">
        {description}
      </p>
    </div>
  )
}

function ModuleBlock({
  icon,
  title,
  description,
}: {
  icon: ReactNode
  title: string
  description: string
}) {
  return (
    <div className="border border-border bg-background p-5">
      <div className="flex items-start gap-4">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-secondary">
          {icon}
        </div>
        <div>
          <h3 className="text-base font-medium">{title}</h3>
          <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
            {description}
          </p>
        </div>
      </div>
    </div>
  )
}

function PlanComparisonTable({ plans }: { plans: SubscriptionPlanDto[] }) {
  if (plans.length === 0) {
    return null
  }

  return (
    <div className="mt-12 overflow-x-auto border border-border">
      <table className="w-full min-w-[760px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-border bg-secondary/60">
            <th className="px-4 py-4 text-left text-xs uppercase tracking-[0.2em] text-muted-foreground">
              Capability
            </th>
            {plans.map((plan) => (
              <th
                key={plan.code}
                className="px-4 py-4 text-left text-xs uppercase tracking-[0.2em] text-muted-foreground"
              >
                {plan.name}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {comparisonQuotaKeys.map((quotaKey) => (
            <tr key={quotaKey} className="border-b border-border">
              <td className="px-4 py-4 text-muted-foreground">
                {getSubscriptionQuotaLabel(quotaKey)}
              </td>
              {plans.map((plan) => (
                <td key={`${plan.code}-${quotaKey}`} className="px-4 py-4">
                  {getPlanQuotaValue(plan, quotaKey)}
                </td>
              ))}
            </tr>
          ))}
          {comparisonFeatureKeys.map((featureKey) => (
            <tr key={featureKey} className="border-b border-border last:border-b-0">
              <td className="px-4 py-4 text-muted-foreground">
                {getSubscriptionFeatureLabel(featureKey)}
              </td>
              {plans.map((plan) => {
                const feature = plan.features.find(
                  (item) => item.key === featureKey,
                )

                return (
                  <td key={`${plan.code}-${featureKey}`} className="px-4 py-4">
                    {feature?.isEnabled ? "Included" : "Not included"}
                  </td>
                )
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function getPlanQuotaValue(
  plan: SubscriptionPlanDto,
  quotaKey: SubscriptionQuotaKey,
): string {
  return formatSubscriptionLimit(
    plan.quotas.find((quota) => quota.key === quotaKey)?.limit,
  )
}
