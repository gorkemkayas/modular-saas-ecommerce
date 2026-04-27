# Order Module Design

## Purpose

This document defines a production-oriented `Order` module design for the current
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

The goal is to model `Order` as the commercial source of truth for purchase
transactions while keeping future modules such as `Inventory`, `Payment`,
`Shipment`, and `Notification` properly decoupled.

---

## Executive Summary

Recommended direction:

- create a separate `Order` module
- make `Order` the aggregate root
- make `OrderItem` a child entity of `Order`
- store customer, address, and price data as order-time snapshots
- separate order lifecycle from payment lifecycle and fulfillment lifecycle
- integrate with `Pricing`, `Inventory`, `Payment`, and `Shipment` through
  application contracts, not direct domain coupling
- treat `Order` as the owner of commercial transaction history, not the owner of
  stock, payment capture, or delivery execution

This gives a clean separation:

- `Catalog` answers: "What is being sold?"
- `Pricing` answers: "At what price is it sold right now?"
- `Order` answers: "What was actually purchased, by whom, and under which terms?"
- `Inventory` answers: "Can it be reserved, allocated, and deducted?"
- `Payment` answers: "Was the money authorized, captured, refunded, or failed?"
- `Shipment` answers: "How is the order physically fulfilled and delivered?"

---

## Business Scope

The `Order` module is responsible for:

- order creation
- order identity and numbering
- order item composition
- customer snapshot and address snapshots
- price snapshot persistence
- order totals
- order lifecycle rules
- cancellation rules
- exposing order history to customers and store admins
- recording references to external operational processes

The `Order` module is not responsible for:

- live product catalog ownership
- live pricing ownership
- stock quantity ownership
- warehouse allocation logic
- payment gateway integrations
- shipment carrier integrations
- email/SMS/push delivery
- advanced promotion engine logic in phase 1
- tax engine logic in phase 1

---

## Why Order Must Be Separate

If order data is mixed into `Catalog`, `Customer`, or `Payment`, the system loses a
clear commercial record of what actually happened at purchase time.

An order is not just a temporary basket or a payment attempt. It is a durable
business record that must survive:

- catalog name changes
- SKU metadata updates
- pricing changes
- customer profile edits
- address edits
- stock changes
- payment retries
- shipment retries

For that reason, the `Order` module must store its own immutable commercial
snapshots and lifecycle history.

---

## Current Solution Constraints

The design must fit the existing codebase:

- storefront product browsing already exists
- pricing resolution already exists
- customer profile and address management already exist
- each module owns its own schema and persistence layer
- Host registers modules via `ECommerce.API/Extensions/*ModuleRegistration.cs`
- MediatR assemblies are registered in `Program.cs`
- application handlers typically use repository + `IUnitOfWork`
- query side typically uses dedicated read services

This means the `Order` module should follow the same structure and language as the
existing modules instead of introducing a new architectural style.

---

## Bounded Context Definition

### Catalog

Owns:

- product identity
- product slug
- SKU and variant structure
- naming, media, attributes, categories
- publishable merchandise data

Does not own:

- sell price snapshots
- order item history
- purchased quantity history

### Pricing

Owns:

- price lists
- resolved live sell prices
- currency-aware commercial pricing rules

Does not own:

- order creation
- order totals history
- order item price persistence after purchase

### Customer

Owns:

- customer profile
- active addresses
- customer preferences and consents

Does not own:

- historical order customer snapshot
- historical order address snapshot

### Order

Owns:

- commercial purchase transaction
- order number
- order items
- price snapshot at order time
- customer snapshot at order time
- billing and shipping address snapshots at order time
- order totals
- order cancellation state
- links to payment, shipment, and inventory processes

Does not own:

- live stock quantity
- payment authorization/capture internals
- shipment packages and tracking ownership
- notification delivery state

### Inventory

Should own later:

- stock item identity
- available quantity
- reservation lifecycle
- deduction and release rules

### Payment

Should own later:

- payment attempts
- authorization
- capture
- refund workflows
- gateway-side references

### Shipment

Should own later:

- fulfillment shipments
- package composition
- carrier information
- tracking events
- delivery outcomes

### Notification

Should own later:

- email/SMS/push templates
- delivery attempts
- channel-specific delivery records

---

## Core Design Principle

The central principle is:

`Order` is the commercial truth.
Other modules are operational specialists.

This means:

- `Order` records what the customer agreed to buy
- `Inventory` decides whether and how stock is reserved or deducted
- `Payment` decides whether money moved successfully
- `Shipment` decides how fulfillment and delivery happen
- `Notification` reacts to events but does not own order state

`Order` may react to results from those modules, but it must not absorb their
internal models.

---

## Aggregate Design

### Aggregate Root

Use `Order` as the aggregate root.

Reason:

- order lifecycle rules are centered in one business record
- order item changes must be coordinated within one transactional boundary
- cancellation and completion rules apply across the order as a whole

### Child Entities

Use `OrderItem` as a child entity of `Order`.

Optional later child entities if needed:

- `OrderStatusHistoryEntry`
- `OrderNote`
- `OrderPaymentReference`
- `OrderShipmentReference`

Phase 1 does not need those as separate entities unless the use cases demand them.

---

## Order Aggregate Responsibilities

`Order` should protect these invariants:

- an order must belong to exactly one store
- an order must belong to exactly one customer
- an order must contain at least one item
- all monetary values in the order must use one currency
- an order cannot be cancelled once terminal completion rules disallow it
- a cancelled order cannot move back into operational progress without an explicit
  reopen policy
- order totals must always equal the sum of item and extra charge snapshots

The aggregate should expose intention-revealing methods such as:

- `Place(...)`
- `Cancel(...)`
- `MarkPaymentAuthorized(...)`
- `MarkPaymentCaptured(...)`
- `MarkPaymentFailed(...)`
- `MarkReservationConfirmed(...)`
- `MarkReservationReleased(...)`
- `MarkShipmentCreated(...)`
- `MarkShipped(...)`
- `MarkDelivered(...)`
- `Complete(...)`

Phase 1 does not need full implementation of all methods, but the aggregate design
should leave room for them.

---

## Proposed Domain Model

### Order

Suggested fields:

- `Id`
- `StoreId`
- `CustomerId`
- `OrderNumber`
- `Status`
- `PaymentStatus`
- `FulfillmentStatus`
- `CurrencyCode`
- `CustomerSnapshot`
- `BillingAddressSnapshot`
- `ShippingAddressSnapshot`
- `IReadOnlyCollection<OrderItem>`
- `OrderTotals`
- `PlacedAtUtc`
- `CancelledAtUtc`
- `CancellationReason`
- `ReservationReference`
- `PaymentReference`
- `ShipmentReference`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### OrderItem

Suggested fields:

- `Id`
- `OrderId`
- `ProductId`
- `ProductVariantId`
- `Sku`
- `ProductName`
- `VariantName`
- `Quantity`
- `UnitPriceSnapshot`
- `LineSubtotalAmount`
- `LineDiscountAmount`
- `LineTaxAmount`
- `LineTotalAmount`

The item should contain enough data to survive future catalog and pricing changes.

### OrderNumber

Use a dedicated value object for external order identity.

Reason:

- external references should not rely on raw `Guid`
- format can evolve later
- store-facing and customer-facing references become clearer

Possible format directions:

- sequential global number
- date-based prefix
- store-scoped sequence

Phase 1 can start with a simple deterministic generator as long as the abstraction
allows replacement later.

### CustomerSnapshot

Suggested fields:

- `CustomerId`
- `Email`
- `FullName`
- `PhoneNumber`

### AddressSnapshot

Suggested fields:

- `Title`
- `ContactName`
- `PhoneNumber`
- `Country`
- `City`
- `District`
- `Line1`
- `Line2`
- `PostalCode`

### OrderPriceSnapshot

Suggested fields:

- `Amount`
- `CurrencyCode`
- `CompareAtAmount`
- `PriceListId`
- `PriceEntryId`

This snapshot should be copied from pricing resolution during order placement.

### OrderTotals

Suggested fields:

- `SubtotalAmount`
- `DiscountAmount`
- `ShippingAmount`
- `TaxAmount`
- `GrandTotalAmount`

In phase 1, `DiscountAmount`, `ShippingAmount`, and `TaxAmount` may be zero while
the model still keeps them explicitly for later growth.

---

## Snapshot Strategy

The `Order` module should never depend on live reads to reconstruct commercial
history after the order is placed.

Snapshot what matters at order time:

- product name
- variant name
- SKU
- resolved unit price
- currency
- pricing source identifiers
- customer display information
- billing address
- shipping address

Do not rely on:

- current catalog name
- current customer address
- current price list entry

This is essential for auditability and for stable historical reads.

---

## Lifecycle Model

Do not represent the entire business lifecycle with one oversized enum.

Use separate dimensions:

### OrderStatus

Suggested values:

- `Pending`
- `Confirmed`
- `Cancelled`
- `Completed`

Meaning:

- `Pending`: order exists but business flow is not fully secured yet
- `Confirmed`: order has passed the minimum success criteria for the business
- `Cancelled`: order was intentionally voided
- `Completed`: commercial lifecycle is finished

### PaymentStatus

Suggested values:

- `Pending`
- `Authorized`
- `Captured`
- `Failed`
- `Refunded`

### FulfillmentStatus

Suggested values:

- `Unfulfilled`
- `PartiallyFulfilled`
- `Fulfilled`
- `Shipped`
- `Delivered`
- `Returned`

This separation keeps the model extensible when payment and fulfillment evolve
independently.

---

## Phase 1 Order Flow

Recommended initial order flow:

1. customer submits an order request
2. application validates store, customer context, and address ownership
3. application resolves live prices from `Pricing`
4. application optionally verifies or reserves stock through `Inventory`
5. aggregate creates immutable snapshots and totals
6. order is persisted
7. `OrderPlaced` domain or integration event is raised
8. downstream modules react

Phase 1 recommended status behavior:

- create order as `Pending`
- if reservation and business checks succeed immediately, mark as `Confirmed`
- allow cancellation while not shipped or otherwise terminal

If payment is not yet implemented, keep `PaymentStatus = Pending`.

---

## Cross-Module Integration Design

Cross-module orchestration belongs in the application layer, not the domain layer.

The domain should never call:

- repositories from other modules
- EF Core queries
- HTTP clients
- payment gateways
- stock services

Instead, use application contracts.

### Pricing Integration

`Order` needs a pricing contract that resolves the commercial unit price to be
snapshotted into order items.

Recommended contract shape:

- resolve price for `(StoreId, ProductId, ProductVariantId, CurrencyCode)`
- return amount, compare-at amount, currency, price list id, and price entry id

Important note:

the current pricing contract surface appears to expose coverage validation, but
not a dedicated resolved price module contract. Before or during `Order`
implementation, expose a clean pricing resolution contract for cross-module use.

### Inventory Integration

`Order` should not own stock math.

Recommended `Inventory` contract directions:

- `CheckAvailability`
- `Reserve`
- `ReleaseReservation`
- `ConfirmDeduction`

Phase 1 options:

- if inventory is not implemented yet, use a no-op adapter
- if overselling is unacceptable, define a minimal reservation contract before
  enabling order placement in production

### Payment Integration

`Order` should not embed payment gateway logic.

Recommended `Payment` contract directions:

- `AuthorizePayment`
- `CapturePayment`
- `CancelPayment`
- `RefundPayment`

`Order` should only store business-level status and references:

- payment reference id
- payment status
- timestamps if useful later

### Shipment Integration

`Order` should not own package and carrier logic.

Recommended `Shipment` contract directions:

- `CreateShipment`
- `MarkPacked`
- `MarkShipped`
- `MarkDelivered`

`Order` can store shipment references and react to shipment outcomes.

### Notification Integration

`Notification` should subscribe to order events such as:

- `OrderPlaced`
- `OrderConfirmed`
- `OrderCancelled`
- `OrderShipped`
- `OrderDelivered`

`Order` should not track notification provider internals.

---

## Recommended Application Abstractions

Suggested abstractions inside `Order.Application`:

- `IOrderRepository`
- `IUnitOfWork`
- `IOrderReadService`
- `IOrderNumberGenerator`
- `IPricingModuleApi` or `IOrderPricingService`
- `IInventoryModuleApi` or `IInventoryReservationService`
- `IPaymentModuleApi`
- `IShipmentModuleApi`
- `ICustomerReadService` or customer snapshot provider contract

Prefer contracts that express `Order` use cases rather than generic infrastructure
helpers.

---

## Commands and Queries

### Phase 1 Commands

- `PlaceOrderCommand`
- `CancelOrderCommand`

### Phase 1 Queries

- `GetMyOrdersQuery`
- `GetOrderByIdQuery`
- `GetStoreOrderByIdQuery`

### Future Commands

- `ConfirmOrderCommand`
- `MarkOrderPaymentAuthorizedCommand`
- `MarkOrderPaymentCapturedCommand`
- `MarkOrderPaymentFailedCommand`
- `MarkOrderShipmentCreatedCommand`
- `MarkOrderShippedCommand`
- `MarkOrderDeliveredCommand`
- `CompleteOrderCommand`
- `RefundOrderCommand`

The initial command set should stay small, but naming should already align with the
future lifecycle.

---

## API Surface Recommendation

Phase 1 customer-facing endpoints:

- `POST /api/orders`
- `GET /api/orders/me`
- `GET /api/orders/me/{orderId}`
- `POST /api/orders/{orderId}/cancel`

Phase 1 store-admin endpoints:

- `GET /api/stores/me/orders/{orderId}`
- later order search endpoints

Keep write operations explicit. Avoid generic patch endpoints for business actions.

---

## Persistence Design

Use a dedicated `OrderDbContext` and a dedicated module schema.

Recommended tables:

- `Orders`
- `OrderItems`

Potential future tables if needed:

- `OrderStatusHistory`
- `OrderNotes`

Start simple unless a concrete use case requires more tables.

Recommended indexing directions:

- `Orders(StoreId, OrderNumber)`
- `Orders(StoreId, CustomerId, CreatedAtUtc)`
- `Orders(StoreId, Status, CreatedAtUtc)`
- `OrderItems(OrderId)`

Keep write-side persistence normalized enough for consistency, but do not over-model
phase 1 history tables without usage.

---

## Query Side Design

Use a dedicated read service for order queries, similar to the other modules.

Suggested read models:

- `OrderSummaryDto`
- `OrderDto`
- `OrderItemDto`

Customer order history should read from order snapshots, not from current customer,
catalog, or pricing state.

That keeps queries stable and avoids accidental cross-module dependency growth.

---

## Authorization and Multi-Tenancy

Recommended rules:

- customer endpoints can only access orders belonging to the current customer and
  tenant/store context
- store-admin endpoints can only access orders belonging to their own store
- no cross-store access
- do not trust raw client-sent `StoreId`

Use tenant context and current user abstractions consistently, following the
existing host patterns.

---

## Domain Events and Integration Events

Even if the project does not yet have a full event infrastructure, design the
module with event boundaries in mind.

Suggested events:

- `OrderPlaced`
- `OrderConfirmed`
- `OrderCancelled`
- `OrderPaymentAuthorized`
- `OrderPaymentCaptured`
- `OrderShipped`
- `OrderDelivered`

These events are useful for:

- notifications
- inventory reactions
- payment reactions
- shipment creation
- future reporting and audit hooks

If event infrastructure is introduced later, the `Order` module should already have
clear event semantics.

---

## Logging and Auditability

Handlers should log key business transitions:

- order placed
- order confirmed
- order cancelled
- payment status changed
- shipment status changed

Include:

- `OrderId`
- `OrderNumber`
- `StoreId`
- `CustomerId`

The existing MediatR logging behavior already provides request-level logging, so
order handlers should add business-level logs for meaningful transitions.

---

## Error Handling

Suggested application exceptions:

- `OrderNotFoundException`
- `OrderValidationException`
- `OrderCancellationNotAllowedException`
- `OrderPricingUnavailableException`
- `OrderInventoryUnavailableException`
- `UnauthorizedOrderAccessException`

Keep exception names explicit and aligned with existing project naming conventions.

---

## Phase Strategy

### Phase 1: Core Order

Implement:

- `Order` aggregate
- `OrderItem`
- snapshots
- totals
- place order
- cancel order
- customer order queries
- admin single order query
- price resolution integration
- optional inventory hook abstraction

Do not implement yet:

- refund workflows
- return workflows
- split shipments
- partial fulfillment
- advanced tax logic
- promotion engine
- complex payment retry logic

### Phase 2: Operational Integration

Add one or more of:

- inventory reservation and release integration
- payment authorization and capture integration
- shipment creation integration
- order confirmation rules tied to operational outcomes

### Phase 3: Commercial Maturity

Possible directions:

- refunds
- returns
- partial shipments
- partial cancellations
- status history
- audit trail enhancements
- reporting projections

---

## Implementation Order

Recommended sequence:

1. `Order.Domain`
   - exceptions
   - enums
   - value objects
   - snapshots
   - `OrderItem`
   - `Order`

2. `Order.Application`
   - abstractions
   - commands
   - queries
   - exceptions
   - integration contracts

3. `Order.Infrastructure`
   - `OrderDbContext`
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
   - pricing resolve contract cleanup
   - inventory reservation integration
   - later payment and shipment hooks

---

## Design Decisions Summary

### Decision 1

`Order` is a dedicated module.

Reason:

- it is a durable business record and a separate bounded context

### Decision 2

`Order` is the aggregate root.

Reason:

- lifecycle and consistency rules must be enforced around the full order

### Decision 3

Order items keep product and price snapshots.

Reason:

- historical accuracy must survive catalog and pricing changes

### Decision 4

Order lifecycle, payment lifecycle, and fulfillment lifecycle are modeled
separately.

Reason:

- avoids collapsing unrelated business dimensions into one enum

### Decision 5

Cross-module orchestration stays in application layer, not domain layer.

Reason:

- preserves module boundaries and domain purity

---

## Final Recommendation

For this repository, the most professional and maintainable `Order` module design is:

- a dedicated module with its own database and lifecycle
- `Order` as aggregate root
- `OrderItem` as child entity
- explicit snapshot value objects for customer, address, price, and totals
- separated order, payment, and fulfillment statuses
- application-layer contracts for `Pricing`, `Inventory`, `Payment`, and `Shipment`
- event-friendly lifecycle boundaries for future `Notification` and operational
  integrations

This gives a strong core now without forcing premature implementation of every
future commerce capability.
