import { Package, Shield, Sparkles, Undo2 } from "lucide-react"

const features = [
  {
    icon: Sparkles,
    title: "Premium Quality",
    description: "Finest fabrics, crafted by masters",
  },
  {
    icon: Package,
    title: "Luxury Packaging",
    description: "Delivered in premium gift boxes",
  },
  {
    icon: Shield,
    title: "Secure Payment",
    description: "Protected with 256-bit SSL encryption",
  },
  {
    icon: Undo2,
    title: "Easy Returns",
    description: "30-day hassle-free return policy",
  },
]

export function FeaturesSection() {
  return (
    <section id="about" className="border-y border-border bg-background py-20 lg:py-24">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="grid gap-12 sm:grid-cols-2 lg:grid-cols-4">
          {features.map((feature) => (
            <div
              key={feature.title}
              className="group flex flex-col items-center text-center"
            >
              <div className="flex h-16 w-16 items-center justify-center border border-border transition-all duration-300 group-hover:border-foreground">
                <feature.icon className="h-6 w-6 text-foreground" strokeWidth={1} />
              </div>
              <h3 className="mt-6 text-[11px] font-normal uppercase tracking-[0.25em] text-foreground">
                {feature.title}
              </h3>
              <p className="mt-3 text-sm leading-relaxed text-muted-foreground">
                {feature.description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
