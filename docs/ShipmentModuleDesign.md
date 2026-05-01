# Shipment Module Design

## Purpose

This document defines a production-oriented `Shipment` module design for the current
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

The goal is to introduce shipment and fulfillment behavior that is detailed enough
to look professional and implementation-worthy, without pretending the current
solution already has a warehouse management system, a rate-shopping engine, or a
carrier orchestration platform.

---

## Executive Summary

Recommended direction:

- create a separate `Shipment` module
- make `Shipment` the aggregate root
- keep `ShipmentLine`, `ShipmentPackage`, and `TrackingEvent` as child entities
- treat shipment as the operational source of truth for outbound fulfillment
- keep `Order` as the commercial source of truth
- integrate with `Order` and `Payment` through contracts, not direct domain coupling
- keep phase 1 outbound-only and store-admin-driven
- support package-level tracking and delivery history from the beginning

This gives a clean separation:

- `Order` answers: "What was purchased?"
- `Payment` answers: "Did the money move successfully?"
- `Inventory` answers: "Was stock reserved and deducted?"
- `Shipment` answers: "How is the order physically dispatched and delivered?"

---

## What The Current Codebase Already Tells Us

Before designing `Shipment`, the existing modules reveal important constraints:

- `Order` already stores `ShippingAddressSnapshot`, `FulfillmentStatus`, and a
  single `ShipmentReference`
- `Inventory` reserves stock during order placement and confirms deduction on
  payment capture
- `Payment` already synchronizes order payment state back into `Order`
- cross-module collaboration is currently handled through module contracts and
  application-layer adapters
- there is no dedicated `Warehouse` module
- there is no physical product metadata such as weight, box dimensions, or
  hazardous-goods flags in `Catalog`

That means `Shipment` should not start by modeling:

- warehouse bins
- pick waves
- cartonization optimization
- shipping rate shopping
- packaging material inventory
- advanced multi-origin routing

Those are reasonable later capabilities, but they are not justified by the current
solution shape.

---

## Business Scope

The `Shipment` module should be responsible for:

- outbound shipment creation for an order
- fulfillment item allocation at shipment level
- package registration
- carrier and service metadata
- tracking number ownership
- tracking event timeline
- shipment lifecycle transitions
- delivery outcome recording
- shipment queries for customers and store admins
- pushing shipment summary state back to `Order`

The `Shipment` module should not be responsible for:

- order pricing or shipping fee calculation
- payment gateway ownership
- stock reservation or stock deduction
- customer address book ownership
- product catalog ownership
- return merchandise authorization in phase 1
- warehouse slotting, picking route optimization, or procurement workflows

---

## Why Shipment Must Be Separate

If shipment data is kept inside `Order`, the order aggregate becomes responsible
for package tracking, carrier workflow, and delivery execution details that are
operationally different from commercial transaction history.

That would make the `Order` aggregate too broad.

`Order` should remain the durable record of:

- who bought
- what was bought
- for how much
- under which address snapshot

`Shipment` should own:

- which items were dispatched
- when they were prepared
- which carrier/service was used
- what tracking references exist
- what delivery events happened

This boundary is especially important because shipment behavior tends to change
much faster than order behavior.

---

## Core Design Principle

The central principle is:

`Order` owns commercial truth. `Shipment` owns fulfillment execution.

This means:

- `Order` stores order-time snapshots and summary fulfillment state
- `Shipment` stores detailed operational fulfillment records
- `Shipment` may update order-level summary flags and references
- `Order` must not absorb package, carrier, and tracking internals

This is the same separation style already used between `Order`, `Inventory`, and
`Payment`.

---

## Recommended Phase 1 Scope

Phase 1 should be detailed, but intentionally bounded.

Implement:

- one shipment record per order in application rules
- shipment-level line allocation
- one or more packages inside the shipment
- carrier name/code and service name/code
- optional tracking numbers
- manual status progression by store admin
- tracking event timeline
- customer and admin shipment queries
- synchronization back to `Order`

Do not implement yet:

- split shipments across multiple independent shipment aggregates for one order
- partial cancellations after part of the order has shipped
- automatic carrier label purchasing
- carrier webhook ingestion from multiple providers
- address validation services
- pickup-point delivery flows
- return workflows

Important note:

The domain model should still be shaped cleanly enough that multiple shipments per
order can be introduced later, but phase 1 should enforce one active shipment per
order because the current `Order` module only keeps a single `ShipmentReference`
and simple summary fulfillment states.

---

## Bounded Context Definition

### Order

Owns:

- commercial purchase record
- order items and price snapshots
- shipping address snapshot
- order lifecycle
- payment status summary
- fulfillment status summary
- shipment summary reference

Does not own:

- carrier selection workflow
- package records
- tracking events
- delivery exception handling details

### Payment

Owns:

- payment method
- authorization/capture/refund state
- gateway references

Does not own:

- dispatch execution
- package tracking
- delivery completion

### Inventory

Owns:

- stock reservation lifecycle
- deduction and release rules

Does not own:

- carrier dispatch
- last-mile delivery

### Shipment

Owns:

- outbound fulfillment execution
- shipment items
- package references
- tracking events
- delivery outcome

Does not own:

- commercial totals
- stock math
- payment gateway state
- customer master profile

---

## Recommended Solution Structure

```text
src/Modules/Shipment/
  Shipment.Application/
  Shipment.Contracts/
  Shipment.Domain/
  Shipment.Infrastructure/
  tests/
    Shipment.Application.UnitTests/
    Shipment.Domain.UnitTests/
```

Host additions:

```text
src/Host/ECommerce.API/
  Contracts/Shipment/
  Controllers/Shipment/
  Extensions/ShipmentModuleRegistration.cs
  ExceptionHandlers/ShipmentExceptionHandler.cs
```

This mirrors the current structure used by `Order`, `Inventory`, and `Payment`.

---

## Ubiquitous Language

Use these terms consistently:

- `Shipment`: one outbound fulfillment record for an order
- `ShipmentLine`: one order item quantity included in the shipment
- `Package`: one physical parcel within the shipment
- `TrackingEvent`: one operational event in the package journey
- `Carrier`: the delivery company or transport provider
- `ServiceLevel`: the carrier service being used
- `Dispatch`: the moment the package leaves operational control

Avoid ambiguous language such as:

- "delivery" when you really mean "shipment creation"
- "tracking" when you really mean "carrier reference only"
- "fulfillment" when you actually mean "package shipping"

---

## Aggregate Design

### Aggregate Root: `Shipment`

Use `Shipment` as the aggregate root.

Reason:

- shipment lifecycle decisions must remain consistent
- package creation and tracking must stay coordinated
- order-level line quantities included in the shipment belong to one
  transactional boundary

Suggested high-level responsibilities:

- create shipment from order context
- allocate order item quantities to shipment lines
- assign carrier metadata
- add packages
- attach tracking references
- register tracking events
- mark shipped
- mark delivered
- cancel before dispatch when allowed

### Child Entities

Use these child entities:

- `ShipmentLine`
- `ShipmentPackage`
- `TrackingEvent`

Why this is the right level of detail:

- `ShipmentLine` preserves what part of the order is being fulfilled
- `ShipmentPackage` models real-world parcels without forcing a separate aggregate
- `TrackingEvent` gives auditability and customer-visible delivery history

Do not create separate aggregates for packages in phase 1.

That would complicate consistency without real benefit in the current solution.

---

## Proposed Domain Model

### Shipment

Suggested fields:

- `Guid Id`
- `Guid StoreId`
- `Guid OrderId`
- `string OrderNumber`
- `string ShipmentNumber`
- `ShipmentStatus Status`
- `string RecipientName`
- `string RecipientPhoneNumber`
- `ShipmentAddress DestinationAddress`
- `string? CarrierCode`
- `string? CarrierName`
- `string? ServiceCode`
- `string? ServiceName`
- `string? TrackingUrl`
- `string? InternalNote`
- `DateTime CreatedAtUtc`
- `DateTime? ReadyForDispatchAtUtc`
- `DateTime? ShippedAtUtc`
- `DateTime? DeliveredAtUtc`
- `DateTime? CancelledAtUtc`
- private `List<ShipmentLine> _lines`
- private `List<ShipmentPackage> _packages`

Notes:

- `ShipmentNumber` should be human-readable and store-facing
- `OrderNumber` should be copied as a snapshot for operational convenience
- destination data should be copied from order snapshot, not referenced live

### ShipmentLine

Suggested fields:

- `Guid Id`
- `Guid ShipmentId`
- `Guid OrderItemId`
- `Guid ProductId`
- `Guid? ProductVariantId`
- `string ProductName`
- `string? VariantName`
- `string? Sku`
- `int Quantity`

Reason:

Shipment should not depend on future live catalog reads to explain what was sent.

### ShipmentPackage

Suggested fields:

- `Guid Id`
- `Guid ShipmentId`
- `string PackageNumber`
- `string? TrackingNumber`
- `decimal? Weight`
- `string? WeightUnit`
- `string? LabelReference`
- `DateTime CreatedAtUtc`
- `DateTime? ShippedAtUtc`
- private `List<TrackingEvent> _trackingEvents`

Important nuance:

`Weight` should be optional because the current catalog does not yet own product
physical dimensions. This lets the module accept manually entered carrier package
data later without making weight a required domain dependency today.

### TrackingEvent

Suggested fields:

- `Guid Id`
- `Guid ShipmentPackageId`
- `TrackingEventType Type`
- `DateTime OccurredAtUtc`
- `string? Location`
- `string Description`
- `string? RawStatusCode`
- `string? RawStatusText`

Reason:

The module should be able to support both:

- manual event entry by admins
- future provider webhook mapping

without losing the original provider status semantics.

---

## Value Objects

### `ShipmentAddress`

Suggested fields:

- `string ContactName`
- `string PhoneNumber`
- `string Country`
- `string City`
- `string District`
- `string Line1`
- `string? Line2`
- `string? PostalCode`

This should closely match the existing order shipping snapshot language.

### `CarrierInfo`

Suggested fields:

- `string CarrierCode`
- `string CarrierName`
- `string? ServiceCode`
- `string? ServiceName`

Use a value object instead of hard-coded carrier enums.

Reason:

- carrier sets evolve
- stores may use local couriers
- string-based normalized codes are more flexible than forcing enum churn

### `TrackingReference`

Optional later value object if the implementation starts using:

- tracking number
- tracking URL
- label reference

Phase 1 can keep those directly on the package/entity if that is simpler.

---

## Domain Enums

### `ShipmentStatus`

Suggested values:

- `Draft = 0`
- `ReadyForDispatch = 1`
- `Shipped = 2`
- `Delivered = 3`
- `DeliveryException = 4`
- `Cancelled = 5`

Why this shape:

- `Draft` allows operational preparation
- `ReadyForDispatch` distinguishes preparation from actual handoff
- `Shipped` marks carrier handoff
- `Delivered` is the happy-path terminal state
- `DeliveryException` handles real failed or blocked outcomes without pretending the
  shipment disappeared
- `Cancelled` supports pre-dispatch cancellation

### `TrackingEventType`

Suggested values:

- `Created = 0`
- `LabelCreated = 1`
- `PickedUp = 2`
- `InTransit = 3`
- `OutForDelivery = 4`
- `Delivered = 5`
- `DeliveryAttemptFailed = 6`
- `Exception = 7`
- `ReturnedToSender = 8`

Keep the enum small and broadly useful.

Do not begin phase 1 with dozens of carrier-specific states.

---

## Domain Invariants

`Shipment` should protect these invariants:

- a shipment must belong to exactly one store
- a shipment must belong to exactly one order
- a shipment must contain at least one shipment line
- all shipment line quantities must be greater than zero
- a shipment cannot be cancelled after it has been shipped
- a delivered shipment cannot be modified
- a shipment cannot be marked shipped without at least one package
- package numbers must be unique within the shipment
- tracking events must be ordered logically by time

Cross-shipment invariants should stay in the application layer, not in the domain
aggregate, because they require knowledge of other shipment records.

Examples:

- whether another shipment already exists for the same order
- whether cumulative fulfilled quantity across shipments exceeds purchased quantity

---

## Shipment Creation Policy

For this codebase, the recommended phase 1 policy is:

- shipment is created only for an existing store order
- shipment uses order shipping snapshot and order item snapshots
- shipment creation is store-admin initiated
- shipment creation is allowed only when the order is operationally eligible

Recommended eligibility for phase 1:

- order is not cancelled
- payment is already captured
- order fulfillment status is not already terminal

Why this conservative rule is good:

- it aligns with the current inventory deduction flow, which happens on payment
  capture
- it avoids premature branch complexity for `CashOnDelivery`
- it keeps fulfillment behavior deterministic for the first implementation

If `CashOnDelivery` becomes a real operational scenario, add a dedicated payment
eligibility contract before widening the fulfillment policy.

---

## Lifecycle Model

Recommended phase 1 lifecycle:

1. store admin creates shipment from an eligible order
2. shipment starts in `Draft`
3. store admin adds packages and carrier metadata
4. shipment moves to `ReadyForDispatch`
5. shipment is marked `Shipped`
6. tracking events accumulate over time
7. shipment becomes `Delivered` or `DeliveryException`

This is intentionally operational and explicit.

Avoid starting with automatic hidden transitions.

Shipment behavior is easier to trust when important business actions are modeled as
explicit commands.

---

## Recommended Domain Methods

Suggested aggregate methods:

- `Create(...)`
- `AssignCarrier(...)`
- `AddPackage(...)`
- `SetInternalNote(...)`
- `MarkReadyForDispatch(...)`
- `MarkShipped(...)`
- `RegisterTrackingEvent(...)`
- `MarkDelivered(...)`
- `MarkDeliveryException(...)`
- `Cancel(...)`

Suggested entity methods:

- `ShipmentPackage.AssignTrackingNumber(...)`
- `ShipmentPackage.AttachLabelReference(...)`
- `ShipmentPackage.AddTrackingEvent(...)`

Keep names intention-revealing and business-oriented.

---

## Cross-Module Integration Design

Cross-module orchestration belongs in the application layer, not the domain layer.

### Order Integration

`Shipment` needs order context to create a shipment.

Recommended new order contract directions:

- `GetStoreOrderShipmentContextAsync`
- `MarkShipmentCreatedAsync`
- `MarkOrderShippedAsync`
- `MarkOrderDeliveredAsync`

Suggested order context contents:

- `OrderId`
- `StoreId`
- `OrderNumber`
- `CustomerId`
- `OrderStatus`
- `PaymentStatus`
- `FulfillmentStatus`
- `ShipmentReference`
- destination shipping address snapshot
- order items with ids, product ids, names, SKU, and quantities

This should mirror the same contract-first pattern already used by `Payment`.

### Payment Integration

`Shipment` should not call gateways or inspect payment internals directly.

It only needs a fulfillment eligibility view.

Recommended shipment-side abstraction:

- `IShipmentPaymentService`

Recommended payment contract direction:

- extend `GetByOrderIdAsync` result to include `PaymentMethodType`
  or
- add a dedicated `GetFulfillmentPaymentContextAsync`

This becomes important if `CashOnDelivery` later allows shipment before capture.

### Inventory Integration

Phase 1 shipment should not manipulate inventory.

Reason:

- order placement already reserves inventory
- payment capture already confirms deduction
- shipment should not duplicate stock ownership rules

Only consider inventory interaction later if you introduce:

- reshipment after delivery loss
- return-to-stock workflows
- shipment void compensation after deduction

### Notification Integration

`Shipment` is a strong future publisher for:

- `ShipmentCreated`
- `ShipmentReadyForDispatch`
- `ShipmentShipped`
- `ShipmentDelivered`
- `ShipmentDeliveryException`

Do not make `Shipment` responsible for email or SMS provider behavior.

---

## Recommended Application Abstractions

Suggested abstractions inside `Shipment.Application`:

- `IShipmentRepository`
- `IUnitOfWork`
- `IShipmentReadService`
- `IShipmentNumberGenerator`
- `IOrderShipmentContextService`
- `IOrderShipmentSyncService`
- `IShipmentPaymentService`

Optional later abstractions:

- `IShipmentCarrierGateway`
- `ITrackingWebhookParser`

Phase 1 should not require a real carrier gateway abstraction unless you already
plan to implement provider APIs immediately.

---

## Commands

### Phase 1 Commands

- `CreateShipmentCommand`
- `AddShipmentPackageCommand`
- `AssignShipmentCarrierCommand`
- `MarkShipmentReadyForDispatchCommand`
- `MarkShipmentShippedCommand`
- `RegisterShipmentTrackingEventCommand`
- `MarkShipmentDeliveredCommand`
- `MarkShipmentDeliveryExceptionCommand`
- `CancelShipmentCommand`

### Future Commands

- `SplitShipmentCommand`
- `CreateReplacementShipmentCommand`
- `AttachCarrierLabelCommand`
- `ImportCarrierTrackingUpdateCommand`

Keep phase 1 command count focused on operational actions that the store admin
actually needs.

---

## Queries

### Customer-facing queries

- `GetMyOrderShipmentsQuery`
- `GetMyOrderShipmentByIdQuery`

### Store-admin queries

- `SearchStoreShipmentsQuery`
- `GetStoreShipmentByIdQuery`
- `GetStoreOrderShipmentsQuery`

Recommended read models:

- `ShipmentSummaryDto`
- `ShipmentDto`
- `ShipmentLineDto`
- `ShipmentPackageDto`
- `TrackingEventDto`

Customer-facing queries should expose package and tracking information, but not
internal operational notes.

---

## API Surface Recommendation

Phase 1 store-admin endpoints:

- `POST /api/stores/me/orders/{orderId}/shipments`
- `GET /api/stores/me/orders/{orderId}/shipments`
- `GET /api/stores/me/shipments`
- `GET /api/stores/me/shipments/{shipmentId}`
- `POST /api/stores/me/shipments/{shipmentId}/packages`
- `PUT /api/stores/me/shipments/{shipmentId}/carrier`
- `POST /api/stores/me/shipments/{shipmentId}/ready`
- `POST /api/stores/me/shipments/{shipmentId}/ship`
- `POST /api/stores/me/shipments/{shipmentId}/tracking-events`
- `POST /api/stores/me/shipments/{shipmentId}/deliver`
- `POST /api/stores/me/shipments/{shipmentId}/delivery-exception`
- `POST /api/stores/me/shipments/{shipmentId}/cancel`

Phase 1 customer endpoints:

- `GET /api/orders/me/{orderId}/shipments`
- `GET /api/orders/me/{orderId}/shipments/{shipmentId}`

This matches the existing API style:

- customer endpoints under `api/orders`
- admin endpoints under `api/stores/me`

---

## Persistence Design

Use a dedicated `ShipmentDbContext` and a dedicated module schema.

Recommended tables:

- `Shipments`
- `ShipmentLines`
- `ShipmentPackages`
- `TrackingEvents`

Suggested indexes:

- unique `(StoreId, ShipmentNumber)`
- `(StoreId, OrderId)`
- `(StoreId, Status, CreatedAtUtc)`
- unique `(ShipmentId, PackageNumber)`
- `(TrackingNumber)`
- `(ShipmentPackageId, OccurredAtUtc)`

This is enough for realistic fulfillment reads without overengineering.

---

## Repository Design

Suggested repository:

- `IShipmentRepository`

Suggested methods:

- `Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);`
- `Task<Shipment?> GetByIdAsync(Guid storeId, Guid shipmentId, CancellationToken cancellationToken = default);`
- `Task<IReadOnlyCollection<Shipment>> ListByOrderIdAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);`
- `Task<bool> ExistsActiveForOrderAsync(Guid storeId, Guid orderId, CancellationToken cancellationToken = default);`

Keep list/search-heavy behavior in read services instead of aggregate repository
methods.

---

## Query Side Design

Use a dedicated read service for shipment queries.

Suggested query responsibilities:

- search shipments by status, order number, shipment number, tracking number
- load full shipment detail with packages and tracking events
- load customer-visible shipment detail scoped by order ownership

Recommended search filters:

- `Status`
- `OrderId`
- `OrderNumber`
- `ShipmentNumber`
- `TrackingNumber`

This gives store admins a genuinely useful operational surface without building a
full analytics module.

---

## Exception Strategy

Follow the same pattern used by the other modules.

### Domain base exception

- `ShipmentDomainException`

### Application base exception

- `Shipment.Application.Exceptions.ApplicationException`

### Suggested application exceptions

- `ShipmentNotFoundException`
- `ShipmentAlreadyExistsForOrderException`
- `ShipmentCreationNotAllowedException`
- `ShipmentDispatchNotAllowedException`
- `ShipmentCancellationNotAllowedException`
- `ShipmentTrackingValidationException`
- `UnauthorizedShipmentAccessException`

### Host exception handler

- `ShipmentExceptionHandler`

Recommended mappings:

- not found -> `404`
- duplicate or conflict -> `409`
- validation or lifecycle rule violations -> `400`
- unauthorized customer/order access -> `401` or `403` according to existing style

---

## Logging and Auditability

Handlers should log key business transitions:

- shipment created
- package added
- shipment marked ready
- shipment shipped
- shipment delivered
- shipment moved to delivery exception
- shipment cancelled

Include:

- `ShipmentId`
- `ShipmentNumber`
- `StoreId`
- `OrderId`
- `OrderNumber`

This is especially important because shipment operations are operationally sensitive
and often investigated after the fact.

---

## Phase Strategy

### Phase 1: Core outbound shipment

Implement:

- `Shipment` aggregate
- `ShipmentLine`
- `ShipmentPackage`
- `TrackingEvent`
- shipment creation from order context
- package registration
- carrier assignment
- manual shipping and delivery transitions
- order sync integration
- customer and admin queries

Do not implement yet:

- multiple active shipments per order
- package-content mapping
- automated label creation
- carrier webhook ingestion
- return logistics

### Phase 2: Operational maturity

Add one or more of:

- multiple shipments per order
- partial fulfillment synchronization back to order
- carrier webhook mapping
- label reference storage
- richer exception workflows

### Phase 3: Advanced fulfillment

Possible directions:

- multi-origin shipping
- pickup point support
- address validation
- rate shopping
- return and exchange logistics

---

## Implementation Order

Recommended sequence:

1. `Shipment.Domain`
   - exceptions
   - enums
   - value objects
   - `TrackingEvent`
   - `ShipmentPackage`
   - `ShipmentLine`
   - `Shipment`

2. `Shipment.Application`
   - abstractions
   - integration contracts
   - commands
   - queries
   - exceptions

3. `Shipment.Contracts`
   - module API
   - request/result records for cross-module usage

4. `Shipment.Infrastructure`
   - `ShipmentDbContext`
   - configurations
   - repository
   - read services
   - DI registration
   - order/payment integration adapters
   - migration

5. `Host`
   - module registration
   - MediatR assembly registration
   - exception handler
   - API contracts
   - controllers
   - appsettings entries

6. integration refinements
   - order contract expansion for shipment context
   - payment fulfillment eligibility contract

---

## Design Decisions Summary

### Decision 1

`Shipment` is a dedicated module.

Reason:

- fulfillment execution is materially different from commercial order ownership

### Decision 2

`Shipment` is the aggregate root.

Reason:

- package, tracking, and lifecycle consistency belong in one fulfillment boundary

### Decision 3

Phase 1 allows one shipment per order in application rules.

Reason:

- this fits the current `Order` module and avoids premature multi-shipment
  complexity

### Decision 4

Packages and tracking events are first-class citizens from day one.

Reason:

- that is the minimum level of detail that makes the module professionally useful

### Decision 5

Shipment does not own shipping price, stock deduction, or payment capture logic.

Reason:

- those responsibilities already belong to `Order`, `Inventory`, and `Payment`

---

## Final Recommendation

For this repository, the most professional and maintainable `Shipment` module
design is:

- a dedicated module with its own database and lifecycle
- `Shipment` as aggregate root
- `ShipmentLine`, `ShipmentPackage`, and `TrackingEvent` as child entities
- manual-first outbound fulfillment in phase 1
- package-level tracking detail from the start
- application-layer contracts with `Order` and `Payment`
- no inventory ownership duplication
- no premature warehouse or carrier-platform overengineering

This gives the project a shipment capability that is:

- realistic
- implementation-friendly
- aligned with the current codebase
- ready to grow later without forcing unnecessary complexity now
