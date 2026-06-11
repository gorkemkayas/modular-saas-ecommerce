# Docker Production Guide

Bu dokuman, bu repo icindeki `ECommerce.API` ve `frontend` uygulamalarini Docker ile production'a almak icindir.

## Neler eklendi

- `src/Host/ECommerce.API/Dockerfile`
- `frontend/Dockerfile`
- `docker-compose.production.yml`
- `.env.production.example`
- `infra/docker/postgres/init-multiple-databases.sh`
- `infra/docker/nginx/default.conf`

## Bu compose neyi kaldirir

- `ecommerce-db` -> PostgreSQL
- `ecommerce-api` -> ASP.NET Core API
- `ecommerce-frontend` -> Next.js frontend
- `nginx` -> `shop` ve `api` subdomain reverse proxy

## AuthService konusunda kritik not

Bu repoda `AuthService` kodu yok. Dolayisiyla bu compose dosyasi `AuthService API` ve `Auth frontend` container'larini build etmez.

Su anki kurgu:

- `ecommerce-api` -> `AuthService__BaseUrl` ile disaridaki veya ayri stack'teki AuthService API'ye baglanir
- `frontend` -> `AUTH_SERVICE_BASE_URL` ile disaridaki veya ayri stack'teki AuthService API'ye baglanir

Yani `identity.kayas.dev` ve `accounts.kayas.dev` icin AuthService repo tarafinda benzer bir Docker stack kurman gerekecek.

## Baslangic akisi

1. Ornek env dosyasini kopyala

```powershell
Copy-Item .env.production.example .env.production
```

2. `.env.production` icini production degerlerinle doldur

3. Container'lari build edip kaldir

```powershell
docker compose --env-file .env.production -f docker-compose.production.yml up -d --build
```

4. Loglari kontrol et

```powershell
docker compose --env-file .env.production -f docker-compose.production.yml logs -f ecommerce-api
docker compose --env-file .env.production -f docker-compose.production.yml logs -f ecommerce-frontend
docker compose --env-file .env.production -f docker-compose.production.yml logs -f nginx
```

## Domain eslestirmesi

`infra/docker/nginx/default.conf` dosyasi su an:

- `shop.kayas.dev`
- `api.kayas.dev`

icin hazir geldi. DNS'te bu iki subdomain ayni sunucu IP'sine gitmelidir.

Farkli domain kullanacaksan bu dosyadaki `server_name` degerlerini degistir.

## HTTPS

Bu compose dosyasi sadece `80` portunda HTTP aciyor.

Production'da iki yol var:

1. Sunucu uzerinde Nginx + Certbot ile SSL terminasyonu
2. Cloudflare proxy kullanip SSL'i orada yonetmek

MVP icin yaygin secim:

- Cloudflare DNS
- VPS uzerinde Docker stack
- ayrica host seviyesinde SSL terminasyonu

## Local veriyi production'a tasima

Bu compose veritabani container'ini hazirlar ama local verileri otomatik tasimaz.

Su akisi kullan:

1. local PostgreSQL'den her modül DB icin dump al
2. production stack'i kaldir
3. production PostgreSQL container'ina restore et

Ornek:

```powershell
docker cp .\backups\ecommerce_store.dump ecommerce-db:/tmp/ecommerce_store.dump
docker exec -it ecommerce-db pg_restore --clean --if-exists --no-owner -U postgres -d ecommerce_store /tmp/ecommerce_store.dump
```

Bu islemi tum modül veritabanlari icin tekrarlarsin.

## Data Protection

Payment modulu sifreli credential sakladigi icin `DataProtection__KeysPath` volume ile kalici tutulur.

Bu volume silinirse daha once sifrelenmis bazı veriler cozulmeyebilir. O nedenle production'da bu volume korunmali.

## Gerekli bir sonraki adim

Bu repo icin Docker tarafi hazir. Sonraki adim:

1. `.env.production` dosyasini gercek secret'larla doldurmak
2. AuthService repo icin ayni mantikla ayri bir Docker stack hazirlamak
3. `accounts.kayas.dev` ve `identity.kayas.dev` icin reverse proxy/SSL eklemek
