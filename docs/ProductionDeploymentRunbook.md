# Production Deployment Runbook

Bu runbook, mevcut yapıyı bozmadan MVP'yi canlıya almak icin hazirlandi.

## 1. Onerilen MVP topolojisi

Tek bir Linux VPS uzerinden baslamak en pratik secim:

- 1 x Ubuntu 24.04 sunucu
- 1 x PostgreSQL sunucusu
  - tercihen managed PostgreSQL
  - ilk yayin icin ayni VPS uzerinde de calisabilir
- 1 x Nginx reverse proxy
- 4 uygulama prosesi
  - `ECommerce.API`
  - `frontend` (e-commerce frontend)
  - `AuthService API`
  - `AuthService frontend`

## 2. Domain yapisi

Kafa karistirmayan ve cookie davranisini sade tutan yapi:

- `shop.kayas.dev` -> e-commerce frontend
- `api.kayas.dev` -> ECommerce.API
- `accounts.kayas.dev` -> AuthService frontend
- `identity.kayas.dev` -> AuthService API

Notlar:

- Musteri auth cookie'lerini `.kayas.dev` ortak domaininde paylastirmayin.
- Mevcut frontend proxy yapisi nedeniyle customer session cookie'lerinin host-only olarak `shop.kayas.dev` uzerinde kalmasi daha temiz.
- `AuthService` token issuer degeri ile `ECommerce.API` tarafindaki `Jwt__Issuer` birebir ayni olmali.

## 3. Bu repoda production icin zorunlu ayarlar

### ECommerce.API

Asagidaki degerler production ortaminda set edilmeli:

- `ASPNETCORE_ENVIRONMENT=Production`
- `Frontend__BaseUrl=https://shop.kayas.dev`
- `Cors__AllowedOrigins__0=https://shop.kayas.dev`
- `AuthService__BaseUrl=https://identity.kayas.dev`
- `AuthService__RegisterPath=/api/v1/auth/register`
- `AuthService__LoginPath=/api/v1/auth/login`
- `AuthService__RefreshPath=/api/v1/auth/refresh`
- `AuthService__ApiKey=...`
- `Jwt__Issuer=https://identity.kayas.dev`
- `Jwt__Audience=tenant-api`
- `Jwt__Secret=...`
- `ServiceTokens__ECommerce=...`
- `CloudinaryMediaStorage__CloudName=...`
- `CloudinaryMediaStorage__ApiKey=...`
- `CloudinaryMediaStorage__ApiSecret=...`
- `Modules__Notification__Email__ApiKey=...`
- `Modules__Notification__Email__WebhookSecret=...`
- `Modules__Payment__Gateway__Provider=Iyzico` veya `Mock`
- `Modules__Payment__Iyzico__Environment=Production`
- `Modules__Payment__Iyzico__CallbackUrl=https://api.kayas.dev/api/payments/callbacks/iyzico/checkout-form`

Veritabani connection string'leri modül bazli set edilmeli:

- `Modules__Store__Database__ConnectionString`
- `Modules__Catalog__Database__ConnectionString`
- `Modules__Customer__Database__ConnectionString`
- `Modules__Pricing__Database__ConnectionString`
- `Modules__Inventory__Database__ConnectionString`
- `Modules__Notification__Database__ConnectionString`
- `Modules__Order__Database__ConnectionString`
- `Modules__Payment__Database__ConnectionString`
- `Modules__Shipment__Database__ConnectionString`
- `Modules__Subscription__Database__ConnectionString`

### Data Protection

Payment modulu provider credential'larini `IDataProtection` ile sakliyor. Bu nedenle key ring kalici olmazsa deploy veya restart sonrasinda sifreli payment credential'lari cozulmeyebilir.

Production'da mutlaka set edin:

- `DataProtection__ApplicationName=ECommerce.API`
- `DataProtection__KeysPath=/var/lib/ecommerce-api/keys`

Bu repo icinde destek eklendi. `KeysPath` bos birakilirsa mevcut davranis devam eder.

### frontend

- `NODE_ENV=production`
- `NEXT_PUBLIC_API_BASE_URL=https://api.kayas.dev`
- `NEXT_PUBLIC_DEFAULT_STORE_SLUG=senin-default-store-slug`
- `AUTH_SERVICE_BASE_URL=https://identity.kayas.dev`
- `NEXT_PUBLIC_AUTH_SERVICE_BASE_URL=https://identity.kayas.dev`
- `AUTH_SERVICE_TENANT_REGISTER_PATH=/api/v1/tenants/register`
- `AUTH_SERVICE_STORE_OWNER_REGISTER_PATH=/api/v1/tenants/register`
- `AUTH_SERVICE_API_KEY=...`

## 4. AuthService tarafinda kontrol etmen gerekenler

Bu repo AuthService kodunu icermiyor ama production oncesi sunlar net olmali:

- `identity.kayas.dev` uzerinde API yayinda olmali
- `accounts.kayas.dev` uzerinde frontend yayinda olmali
- login, register, refresh endpoint path'leri production ile birebir ayni olmali
- token issuer production'da sabit olmali
- refresh token rotation acik olmali
- API key gerekiyorsa frontend proxy ile uyumlu olmali
- AuthService veritabani ayri dump/restore ile tasinmali

## 5. Local veriyi production'a tasima

Bu projede tek veritabani yok. Ayrica her modül icin ayri PostgreSQL baglantisi var. Yani dump/restore isini modül modül yapacaksin.

Onerilen yontem:

1. Her veritabanini `pg_dump -Fc` ile export et
2. Production tarafinda bos veritabanlarini olustur
3. `pg_restore` ile geri yukle
4. Sonrasinda uygulamayi production ayarlariyla kaldir
5. Uygulama acilisinda migration ve seed davranisini loglardan dogrula

### Ornek dump komutlari

```powershell
$databases = @(
  "ecommerce_store",
  "ecommerce_catalog",
  "ecommerce_customer",
  "ecommerce_pricing",
  "ecommerce_inventory",
  "ecommerce_notification",
  "ecommerce_order",
  "ecommerce_payment",
  "ecommerce_shipment",
  "ecommerce_subscription",
  "auth_service"
)

foreach ($db in $databases) {
  pg_dump -Fc --dbname "postgresql://USER:PASSWORD@LOCALHOST:5432/$db" -f ".\\backups\\$db.dump"
}
```

### Ornek restore komutlari

```powershell
$databases = @(
  "ecommerce_store",
  "ecommerce_catalog",
  "ecommerce_customer",
  "ecommerce_pricing",
  "ecommerce_inventory",
  "ecommerce_notification",
  "ecommerce_order",
  "ecommerce_payment",
  "ecommerce_shipment",
  "ecommerce_subscription",
  "auth_service"
)

foreach ($db in $databases) {
  pg_restore --clean --if-exists --no-owner --dbname "postgresql://USER:PASSWORD@PRODHOST:5432/$db" ".\\backups\\$db.dump"
}
```

Notlar:

- `pg_dump` ile `pg_restore` resmi PostgreSQL araclaridir.
- `-Fc` custom format, restore acisindan daha esnektir.
- Farkli makineye tasimada dump/restore, file-level kopyaya gore daha guvenlidir.

## 6. DNS

Cloudflare kullaniyorsan su kayitlar yeterli:

- `shop` -> VPS IP
- `api` -> VPS IP
- `accounts` -> VPS IP
- `identity` -> VPS IP

Hepsi ayni sunucuya gidip Nginx ile ilgili prosese yonlenebilir.

## 7. Nginx yonlendirme mantigi

Mantik su:

- `shop.kayas.dev` -> `127.0.0.1:3000`
- `api.kayas.dev` -> `127.0.0.1:8080`
- `accounts.kayas.dev` -> `127.0.0.1:3001`
- `identity.kayas.dev` -> `127.0.0.1:8081`

Bu portlar ornek. Onemli olan, her servisin local portta ayakta olup Nginx'in domain bazli reverse proxy yapmasi.

## 8. Yayin sirası

En duz ve dusuk riskli sira:

1. Production PostgreSQL veritabanlarini hazirla
2. Local verilerin dump'larini al
3. Production'a restore et
4. AuthService API'yi yayina al
5. AuthService frontend'i yayina al
6. ECommerce.API'yi yayina al
7. E-commerce frontend'i yayina al
8. DNS ve SSL dogrulamalarini tamamla
9. Login, register, refresh, image upload, email, payment callback akislarini test et

## 9. Go-live checklist

- Tum production env degerleri set edildi
- `Jwt__Issuer` ile AuthService issuer ayni
- `NEXT_PUBLIC_API_BASE_URL` production API'yi gosteriyor
- `AuthService__BaseUrl` production identity API'yi gosteriyor
- `DataProtection__KeysPath` kalici bir dizine ayarlandi
- PostgreSQL restore tamamlandi
- Uygulama loglarinda migration hatasi yok
- Notification seed ve subscription seed calisti
- Cloudinary upload calisiyor
- Email gonderimi calisiyor
- Iyzico callback URL production domaine guncellendi
- Customer login / refresh akisi calisiyor
- Store owner registration akisi calisiyor

## 10. Bu repo icin sonraki mantikli adim

Canliya cikmadan hemen once su iki sey yapilmali:

1. Production environment variable dosyalarini veya systemd service dosyalarini hazirlamak
2. Local PostgreSQL veritabanlarinin tam listesini netlestirip dump/restore komutlarini gercek connection string'lerle doldurmak
