const faviconSvg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32">
  <rect width="32" height="32" fill="#111111"/>
  <text x="16" y="22" text-anchor="middle" fill="#ffffff" font-family="Georgia, serif" font-size="18" font-weight="700">K</text>
</svg>`

const headers = {
  "Cache-Control": "public, max-age=31536000, immutable",
  "Content-Type": "image/svg+xml",
}

export function GET() {
  return new Response(faviconSvg, { headers })
}

export function HEAD() {
  return new Response(null, { headers })
}
