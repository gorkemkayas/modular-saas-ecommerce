import { Header } from "@/components/header"
import { Footer } from "@/components/footer"
import { CheckoutContent } from "@/components/checkout/checkout-content"

export default function CheckoutPage() {
  return (
    <>
      <Header />
      <main className="py-8 lg:py-12">
        <CheckoutContent />
      </main>
      <Footer />
    </>
  )
}
