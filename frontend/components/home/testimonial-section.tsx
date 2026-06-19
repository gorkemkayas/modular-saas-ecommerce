import Image from "next/image"

export function TestimonialSection() {
  return (
    <section className="relative overflow-hidden bg-foreground py-32 lg:py-40">
      {/* Background pattern */}
      <div className="absolute inset-0 opacity-5">
        <div className="absolute left-1/4 top-1/4 h-96 w-96 border border-background" />
        <div className="absolute bottom-1/4 right-1/4 h-64 w-64 border border-background" />
      </div>

      <div className="relative z-10 mx-auto max-w-5xl px-6 lg:px-8">
        <div className="flex flex-col items-center text-center">
          {/* Quote mark */}
          <span className="font-serif text-8xl font-light text-background/10 lg:text-9xl">
            {'"'}
          </span>
          
          <blockquote className="-mt-12">
            <p className="font-serif text-2xl font-light leading-relaxed tracking-wide text-background sm:text-3xl lg:text-4xl">
              We strive to deliver the best service possible.
              You can support us with your feedback.
            </p>
          </blockquote>

          <div className="mt-12 flex flex-col items-center">
            <div className="relative h-16 w-16 overflow-hidden rounded-full">
              <Image
                src="/gorkem-kaya.png"
                alt="Görkem Kaya"
                fill
                className="object-cover"
              />
            </div>
            <p className="mt-6 text-[11px] font-normal uppercase tracking-[0.3em] text-background">
              Görkem Kaya
            </p>
            <p className="mt-2 text-sm text-background/50">
              Founder & Developer
            </p>
          </div>
        </div>
      </div>
    </section>
  )
}
