import { Header } from "@/components/header"
import { Footer } from "@/components/footer"
import { CartContent } from "@/components/cart/cart-content"

export default function CartPage() {
  return (
    <>
      <Header />
      <main className="py-8 lg:py-12">
        <CartContent />
      </main>
      <Footer />
    </>
  )
}
