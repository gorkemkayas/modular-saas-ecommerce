# Pricing Module Design

## Purpose

This document defines a production-oriented `Pricing` module design for the current
`modular-saas-ecommerce` solution.

The design is tailored to the existing project conventions:

- modular monolith
- DDD + Clean Architecture
- CQRS with MediatR
- per-module `Application`, `Domain`, `Infrastructure`
- per-module `DbContext` and `IUnitOfWork`
- repository pattern on write side
- read service pattern on query side
- tenant/store scoping with `Guid StoreId`

The goal is not only to support "a price field", but to establish a pricing domain
that can grow into a professional commerce capability.

---

## Executive Summary

Recommended direction:

- create a separate `Pricing` module
- make `PriceList` the aggregate root
- make `PriceEntry` a child entity of `PriceList`
- represent money with `Money` + `Currency` value objects
- target prices by `ProductId` or `ProductVariantId`
- keep `Catalog` as the owner of product identity and merchandising data
- make `Pricing` the owner of commercial selling price
- integrate with `Catalog` only through application-level contracts, not direct
  domain coupling
- make future `Order` module consume a resolved price snapshot from `Pricing`

This gives a clean separation:

- `Catalog` answers: "What is this thing?"
- `Pricing` answers: "At what price is it sold?"

---

## Business Scope

The `Pricing` module is responsible for:

- store-scoped price lists
- product-level and variant-level fixed prices
- default selling price resolution
- commercial validity of prices
- price list lifecycle
- future expansion into advanced pricing capabilities

The `Pricing` module is not responsible for:

- product naming, categories, media, attributes
- product publication workflow ownership
- customer profile management
- cart behavior
- order creation
- payment processing
- coupon application in phase 1
- tax calculation in phase 1

---

## Why Pricing Must Be Separate

If pricing stays inside `Catalog`, product metadata and commercial rules become mixed.
That works for an MVP, but it becomes expensive when you need:

- multiple prices for one sellable item
- default and alternate price sets
- country or currency expansion
- B2B and B2C price differences
- promotions and scheduled prices
- future channel or segment pricing
- proper order-time price snapshots

A separate `Pricing` module gives:

- cleaner bounded contexts
- better long-term changeability
- explicit commercial ownership
- safer evolution toward promotions and advanced pricing

The tradeoff is higher orchestration cost between modules. That is acceptable here
because the repo is already built as a modular monolith with explicit module boundaries.

---

## Current Solution Constraints

The design must fit the existing codebase:

- `Catalog.Product` and `Catalog.ProductVariant` currently do not own price
- `Catalog` already uses `StoreId` as a store/tenant boundary
- each module owns its own schema and persistence layer
- Host registers modules via `ECommerce.API/Extensions/*ModuleRegistration.cs`
- MediatR assemblies are registered in `Program.cs`
- application handlers typically use repository + `IUnitOfWork`
- query side typically uses dedicated read services

This means the new module should follow the same shape and language.

---

## Bounded Context Definition

### Catalog

Owns:

- product identity
- SKU
- slug
- variant structure
- attributes
- categories
- publishable merchandise data

Does not own:

- sell price
- price list selection
- resolved sale amount

### Pricing

Owns:

- store price lists
- price entries
- currency-aware amounts
- default price resolution
- future pricing policies

Does not own:

- product domain lifecycle
- product descriptive data
- customer segmentation rules in phase 1
- order state

---

## Module Boundary Rules

Hard rules:

- `Pricing.Domain` must not reference `Catalog.Domain`
- `Pricing` should know only these external identifiers:
  - `StoreId`
  - `ProductId`
  - `ProductVariantId`
- external module validation must happen through application contracts
- cross-module business checks must not leak into domain entities

This is important because the domain must stay pure.

---

## Recommended Solution Structure

```text
src/Modules/Pricing/
  Pricing.Application/
  Pricing.Domain/
  Pricing.Infrastructure/
  tests/
    Pricing.Application.UnitTests/
    Pricing.Domain.UnitTests/
    Pricing.Infrastructure.IntegrationTests/
```

Host additions:

```text
src/Host/ECommerce.API/
  Contracts/Pricing/
  Controllers/Pricing/
  Extensions/PricingModuleRegistration.cs
  ExceptionHandlers/PricingExceptionHandler.cs
```

Solution additions:

- `src/Modules/Pricing/Pricing.Application/Pricing.Application.csproj`
- `src/Modules/Pricing/Pricing.Domain/Pricing.Domain.csproj`
- `src/Modules/Pricing/Pricing.Infrastructure/Pricing.Infrastructure.csproj`
- `src/Modules/Pricing/tests/Pricing.Application.UnitTests/Pricing.Application.UnitTests.csproj`
- `src/Modules/Pricing/tests/Pricing.Domain.UnitTests/Pricing.Domain.UnitTests.csproj`
- `src/Modules/Pricing/tests/Pricing.Infrastructure.IntegrationTests/Pricing.Infrastructure.IntegrationTests.csproj`

---

## Ubiquitous Language

Use these terms consistently:

- `PriceList`: a store-scoped collection of prices
- `PriceEntry`: one price definition for one sellable target inside a price list
- `PriceTarget`: identifies what the price applies to
- `Money`: amount + currency
- `ResolvedPrice`: the effective price returned to consuming modules
- `DefaultPriceList`: the primary active list used for ordinary selling

Avoid ambiguous language such as:

- "product price" when the item may actually be a variant
- "discounted price" if it is really a compare-at display amount
- "final price" before promotions, tax, and coupon logic exist

---

## Aggregate Design

### Aggregate Root: `PriceList`

`PriceList` should be the main aggregate root.

Why:

- a store may need multiple price collections over time
- default list selection is a domain concern
- priority and lifecycle belong at the list level
- future campaign/segment/channel pricing can be built on lists

Suggested properties:

- `Guid Id`
- `Guid StoreId`
- `string Name`
- `Currency Currency`
- `int Priority`
- `bool IsDefault`
- `PriceListStatus Status`
- `DateTime CreatedAtUtc`
- `DateTime UpdatedAtUtc`
- private `List<PriceEntry> _entries`

Suggested behavior:

- `Create`
- `Rename`
- `ChangePriority`
- `MarkAsDefault`
- `UnmarkAsDefault`
- `Activate`
- `Deactivate`
- `Archive`
- `SetProductPrice`
- `SetVariantPrice`
- `RemovePrice`
- `ActivatePriceEntry`
- `DeactivatePriceEntry`

### Child Entity: `PriceEntry`

`PriceEntry` should be a child entity inside `PriceList`.

Suggested properties:

- `Guid Id`
- `Guid PriceListId`
- `PriceTarget Target`
- `Money Price`
- `Money? CompareAtPrice`
- `bool IsActive`
- `DateTime CreatedAtUtc`
- `DateTime UpdatedAtUtc`

Why child entity instead of separate aggregate:

- price rows usually change under the governance of a single price list
- invariants like uniqueness per target belong naturally inside the list
- it keeps phase 1 simpler while still extensible

---

## Value Objects

### `Currency`

Suggested shape:

- `string Code`

Rules:

- cannot be null or whitespace
- normalize to upper case
- must be exactly 3 letters
- validate against a strict regex or known ISO-like format rule

Suggested examples:

- `TRY`
- `USD`
- `EUR`

### `Money`

Suggested shape:

- `decimal Amount`
- `Currency Currency`

Rules:

- amount cannot be negative
- currency is required
- arithmetic should require same currency
- phase 1 precision: `decimal(18,2)`

Behavior that is worth adding:

- `Add`
- `Subtract`
- `Multiply`
- comparison operators if same currency

Keep logic explicit. Avoid magic implicit conversions.

### `PriceTarget`

Suggested shape:

- `Guid ProductId`
- `Guid? ProductVariantId`

Meaning:

- `ProductVariantId == null` means product-level target
- `ProductVariantId != null` means variant-level target

Rules:

- `ProductId` is mandatory
- `ProductVariantId` is optional

This lets the module support both:

- simple product pricing
- variant pricing

without coupling to Catalog entities.

---

## Domain Enums

### `PriceListStatus`

Suggested values:

- `Draft = 1`
- `Active = 2`
- `Inactive = 3`
- `Archived = 4`

Reasoning:

- `Draft` for incomplete lists
- `Active` for lists allowed in resolution
- `Inactive` for temporarily disabled lists
- `Archived` for historical but immutable lists

---

## Domain Invariants

### `PriceList` invariants

- `StoreId` cannot be empty
- `Name` cannot be empty
- `Priority` should be within a defined range if you want guardrails
- archived lists cannot be mutated
- only active lists may be used by price resolution
- a list marked default should be active or explicitly activated soon after

### `PriceEntry` invariants

- target product id cannot be empty
- price amount cannot be negative
- compare-at price, if present, cannot be lower than actual price
- the entry currency must match the parent list currency
- the same target cannot appear twice within one list
- inactive entries must be ignored by resolution

### Cross-module business rules

These should not live inside `Pricing.Domain` because they depend on `Catalog`:

- whether the product exists
- whether the variant exists
- whether the variant belongs to the product
- whether the variant is active
- whether the product is `Simple` or `Variant`

These belong in `Pricing.Application`.

---

## Critical Product and Variant Rules

Recommended commercial policy for phase 1:

- `Simple` products use product-level price only
- `Variant` products use variant-level price only
- no product-level fallback for variant products in phase 1

Why this is the better policy:

- resolution is deterministic
- order creation becomes simpler
- fewer hidden fallback bugs
- clearer API contracts
- better domain clarity

Do not start with mixed precedence rules unless you absolutely need them.

---

## Application Layer Design

Suggested structure:

```text
Pricing.Application/
  Abstractions/
  Exceptions/
  Integrations/
  PriceLists/
    Commands/
    DTOs/
    Queries/
  Prices/
    DTOs/
    Queries/
  AssemblyReference.cs
```

### Core abstractions

- `IUnitOfWork`
- `IPriceListRepository`
- `IPriceListReadService`
- `IPriceResolutionReadService`
- `ICatalogSellableItemValidator`

Optional later abstractions:

- `IPriceAvailabilityChecker`
- `IPriceResolver`

### Integration abstraction: `ICatalogSellableItemValidator`

Purpose:

- validate external catalog references without coupling domains

Suggested contract:

```csharp
public interface ICatalogSellableItemValidator
{
    Task<SellableItemValidationResult> ValidateAsync(
        Guid storeId,
        Guid productId,
        Guid? productVariantId,
        CancellationToken cancellationToken = default);
}
```

Suggested result:

```csharp
public sealed record SellableItemValidationResult(
    bool ProductExists,
    bool VariantExists,
    bool VariantBelongsToProduct,
    bool VariantIsActive,
    CatalogSellableItemType ProductType);
```

Host can implement this by using Catalog queries through MediatR.

---

## Commands

### Price list lifecycle commands

- `CreatePriceListCommand`
- `RenamePriceListCommand`
- `ChangePriceListPriorityCommand`
- `ActivatePriceListCommand`
- `DeactivatePriceListCommand`
- `ArchivePriceListCommand`
- `SetDefaultPriceListCommand`

### Price mutation commands

- `SetProductPriceCommand`
- `SetVariantPriceCommand`
- `RemovePriceEntryCommand`
- `ActivatePriceEntryCommand`
- `DeactivatePriceEntryCommand`

### Suggested command behavior

`SetProductPriceCommandHandler` should:

- validate store id
- validate external product reference through `ICatalogSellableItemValidator`
- ensure target is a simple product
- load target `PriceList`
- call aggregate method
- save changes through `IUnitOfWork`

`SetVariantPriceCommandHandler` should:

- validate store id
- validate product + variant relation
- ensure product type is variant
- ensure variant is active
- load target `PriceList`
- call aggregate method
- save changes

---

## Queries

### Administrative queries

- `GetPriceListByIdQuery`
- `SearchPriceListsQuery`
- `SearchPriceEntriesQuery`

### Commerce-facing queries

- `GetResolvedPriceQuery`
- `GetResolvedPricesForProductQuery` if needed later

### `GetResolvedPriceQuery`

This should become the canonical entry point for other modules.

Suggested input:

- `Guid StoreId`
- `Guid ProductId`
- `Guid? ProductVariantId`
- `string CurrencyCode`

Suggested output:

- `ResolvedPriceDto?`

Suggested semantics:

- returns `null` if no valid price can be resolved
- returns only active/default/eligible list results in phase 1

This is what `Order` should call later.

---

## DTO Design

### `PriceListDto`

- `Guid Id`
- `Guid StoreId`
- `string Name`
- `string CurrencyCode`
- `int Priority`
- `bool IsDefault`
- `PriceListStatus Status`
- `DateTime CreatedAtUtc`
- `DateTime UpdatedAtUtc`
- `IReadOnlyCollection<PriceEntryDto> Entries`

### `PriceEntryDto`

- `Guid Id`
- `Guid ProductId`
- `Guid? ProductVariantId`
- `decimal Amount`
- `string CurrencyCode`
- `decimal? CompareAtAmount`
- `bool IsActive`

### `ResolvedPriceDto`

- `Guid StoreId`
- `Guid ProductId`
- `Guid? ProductVariantId`
- `decimal Amount`
- `string CurrencyCode`
- `decimal? CompareAtAmount`
- `Guid PriceListId`
- `Guid PriceEntryId`

This is intentionally order-friendly.

---

## Persistence Design

Suggested structure:

```text
Pricing.Infrastructure/
  DependencyInjection/
  Options/
  Persistence/
    Configurations/
    Migrations/
    Repositories/
    PricingDbContext.cs
  ReadServices/
```

### `PricingDbContext`

Suggested `DbSet`s:

- `DbSet<PriceList> PriceLists`
- `DbSet<PriceEntry> PriceEntries`

`PricingDbContext` should implement `Pricing.Application.Abstractions.IUnitOfWork`.

### Repository

Suggested repository:

- `IPriceListRepository`

Suggested methods:

- `Task AddAsync(PriceList priceList, CancellationToken cancellationToken = default);`
- `Task<PriceList?> GetByIdAsync(Guid storeId, Guid priceListId, CancellationToken cancellationToken = default);`
- `Task<PriceList?> GetDefaultByStoreAndCurrencyAsync(Guid storeId, Currency currency, CancellationToken cancellationToken = default);`
- `Task<bool> ExistsDefaultActiveListAsync(Guid storeId, Currency currency, Guid? excludedPriceListId = null, CancellationToken cancellationToken = default);`

Keep read-heavy search behavior in read services.

### Read services

- `PriceListReadService`
- `PriceResolutionReadService`

`PriceResolutionReadService` should be optimized for lookup and projection rather than
aggregate hydration.

---

## Database Mapping

### `PriceListConfiguration`

Recommended mapping:

- table: `PriceLists`
- key: `Id`
- `StoreId` required
- `Name` max 200
- `Currency` mapped cleanly
- `Priority` required
- `IsDefault` required
- `Status` required
- timestamps required
- child navigation uses field access if needed

### `PriceEntryConfiguration`

Recommended mapping:

- table: `PriceEntries`
- key: `Id`
- `PriceListId` required
- `ProductId` required
- `ProductVariantId` nullable
- `Price.Amount` decimal(18,2)
- `Price.Currency` max 3
- `CompareAtPrice` optional
- `IsActive` required
- timestamps required

### Recommended indexes

On `PriceLists`:

- unique or constrained path for `(StoreId, CurrencyCode, IsDefault)` among active/default lists
- `(StoreId, Status)`
- `(StoreId, CurrencyCode, Priority)`

On `PriceEntries`:

- unique `(PriceListId, ProductId, ProductVariantId)`
- `(ProductId, ProductVariantId)`
- `(PriceListId, IsActive)`

This is enough for phase 1.

---

## Price Resolution Policy

Phase 1 resolution algorithm should stay intentionally simple.

Recommended algorithm:

1. validate input
2. locate active default price list for `(StoreId, Currency)`
3. look for a matching active entry in that list
4. return the resolved row as `ResolvedPriceDto`
5. if none exists, return `null`

For product types:

- simple product: resolve on product-level target
- variant product: resolve on variant-level target

Do not implement multi-list fallback, priority merge, or promotional overlays in phase 1.

That would make the module look bigger, but not necessarily better.

---

## API Design

Suggested route family:

```text
api/stores/me/pricing/lists
api/stores/me/pricing/lists/{priceListId}
api/stores/me/pricing/lists/{priceListId}/default
api/stores/me/pricing/lists/{priceListId}/activate
api/stores/me/pricing/lists/{priceListId}/deactivate
api/stores/me/pricing/lists/{priceListId}/archive
api/stores/me/pricing/lists/{priceListId}/products/{productId}
api/stores/me/pricing/lists/{priceListId}/products/{productId}/variants/{variantId}
```

Suggested controller:

- `StorePriceListsController`

Suggested request contracts:

- `CreatePriceListRequest`
- `RenamePriceListRequest`
- `ChangePriceListPriorityRequest`
- `SetProductPriceRequest`
- `SetVariantPriceRequest`

Use the same tenant-admin approach already used by Catalog store controllers.

---

## Exception Strategy

Follow the same pattern already used by Catalog, Store, and Customer.

### Domain base exception

- `PricingDomainException`

### Application base exception

- `Pricing.Application.Exceptions.ApplicationException`

### Suggested application exceptions

- `PriceListNotFoundException`
- `DuplicateDefaultPriceListException`
- `InvalidPriceTargetException`
- `CatalogSellableItemNotFoundException`
- `VariantPriceNotAllowedException`
- `ResolvedPriceNotFoundException`

### Host exception handler

- `PricingExceptionHandler`

Recommended mappings:

- not found -> `404`
- duplicate/default conflicts -> `409`
- invalid pricing rule / validation -> `400`

---

## Catalog Integration

### Price write integration

When setting a price, `Pricing` should validate the target through
`ICatalogSellableItemValidator`.

That validator should answer:

- does the product exist
- does the variant exist
- does the variant belong to the product
- what is the product type
- is the variant active

### Publish integration

If business wants "a product cannot be published without a valid price", the check should
not be pushed into `Catalog.Domain.Product.Publish()`.

Why:

- that would couple Catalog domain invariants to Pricing
- publish would become a cross-module domain concern

Correct location:

- `Catalog.Application.Products.Commands.PublishProduct.PublishProductCommandHandler`

Recommended abstraction:

- `IProductPricingAvailabilityChecker`

Workflow:

1. load product in Catalog
2. ask Pricing whether valid price coverage exists
3. if yes, call `product.Publish()`
4. persist

This keeps module boundaries clean.

---

## Future Order Integration

The `Order` module should never rely on live pricing at read time after the order is created.

Correct flow:

1. order creation asks `Pricing` for `ResolvedPriceDto`
2. order stores price snapshot inside order items
3. later price changes do not affect existing orders

Snapshot data should include at minimum:

- amount
- currency
- source price list id
- source price entry id if useful for traceability

This is a major reason to keep `Pricing` separate and explicit.

---

## Security and Authorization

Write endpoints should require tenant admin scope, similar to Catalog store endpoints.

Rules:

- store admins manage price lists for their own store
- no cross-store access
- all queries and commands must use `StoreId` from tenant context or explicitly validated admin routes

Do not trust raw client-sent store ids in ordinary store-admin endpoints.

---

## Operational Considerations

### Logging

Handlers should log:

- price list created
- default list changed
- product price set
- variant price set
- price removed

Include:

- `PriceListId`
- `StoreId`
- `ProductId`
- `ProductVariantId` when relevant

### Auditing

If audit support is introduced later, `Pricing` is a strong candidate because price changes
are commercially sensitive.

### Migrations

Use one migration stream per module, same as Catalog and Store.

### Configuration

Add:

- `Pricing.Infrastructure.Options.PricingDatabaseOptions`

Use:

- `Modules:Pricing:Database:ConnectionString`

---

## Phased Delivery Plan

### Phase 1: Core pricing

Implement:

- `PriceList`
- `PriceEntry`
- `Money`
- `Currency`
- `PriceTarget`
- default active list
- product price
- variant price
- resolved price query
- store admin API

Do not implement yet:

- promotions
- coupons
- scheduling
- tax
- segment pricing
- channel pricing

### Phase 2: Commercial maturity

Add one or more of:

- `PriceListType`
- validity window
- country/channel scope
- customer segment scope
- compare-at display policies

### Phase 3: Advanced pricing

Possible directions:

- scheduled price versions
- campaign price lists
- B2B and retail price sets
- loyalty-specific pricing

---

## Implementation Order

Recommended sequence:

1. `Pricing.Domain`
   - exceptions
   - enums
   - value objects
   - `PriceEntry`
   - `PriceList`

2. `Pricing.Application`
   - abstractions
   - repository contract
   - read service contracts
   - commands
   - queries
   - exceptions

3. `Pricing.Infrastructure`
   - `PricingDbContext`
   - configurations
   - repository
   - read services
   - DI registration
   - migration

4. `Host`
   - module registration
   - MediatR assembly registration
   - exception handler
   - API contracts
   - controller
   - appsettings entries

5. integration refinements
   - Catalog publish check
   - later Order price snapshot usage

---

## Design Decisions Summary

### Decision 1

Price is not a field of `Catalog.Product`.

Reason:

- pricing is its own business capability

### Decision 2

`PriceList` is the aggregate root.

Reason:

- better support for future multiple pricing contexts

### Decision 3

Phase 1 uses one active default list per `(StoreId, Currency)`.

Reason:

- simple and deterministic resolution

### Decision 4

Simple products use product-level price, variant products use variant-level price.

Reason:

- explicit behavior, no hidden precedence

### Decision 5

Cross-module validation stays in application layer, not domain layer.

Reason:

- preserves module boundaries and domain purity

---

## Final Recommendation

For this repository, the most professional and maintainable `Pricing` module design is:

- a dedicated module with its own database and lifecycle
- `PriceList` as aggregate root
- `PriceEntry` as child entity
- `Money`, `Currency`, and `PriceTarget` as explicit value objects
- strict product-vs-variant pricing rules
- a simple default-list resolution model in phase 1
- application-level integration with Catalog
- future `Order` integration through resolved price snapshot

This gives you a design that is:

- professional
- implementation-friendly
- aligned with the current codebase
- extensible without premature overengineering

