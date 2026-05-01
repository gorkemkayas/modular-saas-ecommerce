# Notification Module Design

## Purpose

`Notification` should be the module responsible for:

- composing outbound transactional communications
- rendering store-aware templates
- recording delivery attempts and outcomes
- hiding provider-specific email or SMS behavior from the rest of the system

It should **not** own:

- order state
- payment state
- shipment state
- customer identity truth
- customer marketing consent truth

`Notification` is an operational specialist, not a business-truth module.

---

## Core Principle

The central principle is:

`Notification` reacts to business outcomes and delivers communication, but does not decide business state.

This means:

- `Order` decides what was purchased
- `Payment` decides whether money moved
- `Shipment` decides fulfillment and delivery progress
- `Notification` decides whether and how a message was composed, attempted, sent, failed, or suppressed

Other modules should never assemble raw email bodies themselves.
They should trigger `Notification` with a business event or typed request and let this
module own delivery details.

---

## Recommended MVP Scope

For phase 1, keep the scope intentionally narrow:

- transactional notifications only
- email as the first supported channel
- store-scoped templates
- persisted delivery history
- provider abstraction for email sending

Do **not** include these in phase 1:

- marketing campaigns
- bulk audience segmentation
- push notifications
- in-app notification center
- webhook fanout infrastructure
- provider failover routing
- complex scheduling
- outbox-driven orchestration

This keeps the module useful without overbuilding it.

---

## What It Should Notify

Recommended phase 1 triggers:

- `OrderPlaced`
- `OrderCancelled`
- `PaymentAuthorized`
- `PaymentCaptured`
- `PaymentFailed`
- `PaymentRefunded`
- `ShipmentCreated`
- `ShipmentShipped`
- `ShipmentDelivered`
- `ShipmentDeliveryException`

These are the moments where the customer or store operator benefits from explicit communication.

---

## Transactional vs Marketing

This distinction matters.

Phase 1 `Notification` should be **transactional-first**.

Examples:

- order confirmation email
- payment success email
- shipment dispatched email
- delivery completed email

These should not be modeled as marketing communication.

Current `Customer` consent values are:

- `EmailMarketing`
- `SmsMarketing`
- `Profiling`

Those are not enough to drive a full notification permission system.
That is acceptable for MVP, because transactional notifications can be sent from
business context snapshots without introducing campaign logic.

Conclusion:

- use `Customer` consents later for marketing features
- do not block transactional email design on marketing consent infrastructure

---

## Module Boundaries

`Notification` should own:

- template definitions
- template activation/deactivation
- rendered message persistence
- per-channel dispatch records
- delivery attempts
- provider response references
- suppression/failure reasons

`Notification` should read from other modules through contracts or context services:

- customer-facing identity data
- order summary data
- payment status context
- shipment tracking context

`Notification` should not mutate:

- orders
- payments
- shipments
- customer profiles

---

## Aggregate Design

This module should have **two main aggregates**.

### 1. NotificationTemplate

Use `NotificationTemplate` as the aggregate root for content configuration.

It should represent:

- one store
- one trigger
- one channel
- one locale or template variant
- one active/inactive version of subject/body content

Suggested fields:

- `Id`
- `StoreId`
- `Key` or `Trigger`
- `Channel`
- `Locale`
- `Name`
- `SubjectTemplate`
- `BodyTemplate`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Suggested invariants:

- template key must be unique per store + channel + locale
- inactive templates cannot be used for new dispatches
- subject may be optional for SMS, required for email

### 2. NotificationDispatch

Use `NotificationDispatch` as the aggregate root for actual outbound communication.

It should represent one logical message created from one business trigger for one recipient on one channel.

Suggested fields:

- `Id`
- `StoreId`
- `Channel`
- `Trigger`
- `Status`
- `RecipientAddress`
- `RecipientName`
- `Subject`
- `Body`
- `BusinessEntityType`
- `BusinessEntityId`
- `ExternalUserId` or `CustomerId` when available
- `ProviderName`
- `ProviderMessageId`
- `FailureCode`
- `FailureMessage`
- `SuppressionReason`
- `CreatedAtUtc`
- `SentAtUtc`
- `LastAttemptAtUtc`

Suggested child entity:

- `NotificationAttempt`

### NotificationAttempt

This child entity should record:

- attempt number
- provider request reference
- provider response reference
- status
- failure details
- attempted at timestamp

This gives us a clean audit trail without bloating the root aggregate.

---

## Recommended Enums

Suggested enums:

- `NotificationChannel`
  - `Email`
  - later `Sms`
  - later `Push`

- `NotificationTrigger`
  - `OrderPlaced`
  - `OrderCancelled`
  - `PaymentAuthorized`
  - `PaymentCaptured`
  - `PaymentFailed`
  - `PaymentRefunded`
  - `ShipmentCreated`
  - `ShipmentShipped`
  - `ShipmentDelivered`
  - `ShipmentDeliveryException`

- `NotificationStatus`
  - `Pending`
  - `Sent`
  - `Failed`
  - `Suppressed`

- optional later `NotificationSuppressionReason`
  - `MissingRecipient`
  - `MissingTemplate`
  - `ChannelDisabled`
  - `RecipientUnavailable`

For MVP a nullable string suppression reason is acceptable if you want to move faster.

---

## Message Composition

The source modules should not pass fully rendered content.

Instead, `Notification` should:

1. load a business context snapshot
2. load the active template
3. render the template
4. persist the dispatch
5. call the channel provider
6. store the result

This preserves module responsibility.

Recommended application abstractions:

- `INotificationTemplateRepository`
- `INotificationDispatchRepository`
- `INotificationReadService`
- `IUnitOfWork`
- `IEmailGateway`
- later `ISmsGateway`
- `ITemplateRenderer`

Recommended context services:

- `IOrderNotificationContextService`
- `IPaymentNotificationContextService`
- `IShipmentNotificationContextService`

These services should read through module contracts and return notification-specific snapshots.

---

## Notification Context Strategy

Do not make `Notification` depend directly on other modules' internal entities.

Instead, define explicit notification context models such as:

- `OrderNotificationContext`
- `PaymentNotificationContext`
- `ShipmentNotificationContext`

Examples of data those contexts may contain:

### OrderNotificationContext

- order id
- order number
- store id
- customer email
- customer full name
- order total
- currency
- placed at

### PaymentNotificationContext

- payment id
- order id
- order number
- payment status
- amount
- currency
- payment reference
- recipient email

### ShipmentNotificationContext

- shipment id
- order id
- order number
- shipment number
- carrier name
- tracking number
- tracking url
- recipient email
- recipient name

This keeps template rendering clean and explicit.

---

## Delivery Behavior

For MVP, use synchronous provider calls inside the `Notification` module.

Recommended behavior:

1. source module completes its own state change first
2. source module calls `Notification`
3. `Notification` creates a dispatch record
4. `Notification` attempts delivery immediately
5. `Notification` marks the dispatch as `Sent`, `Failed`, or `Suppressed`

Important rule:

`Notification` failure should not roll back commercial truth.

If an order is placed or a payment is captured successfully, the system should not
undo that business state just because an email failed.

That means:

- caller modules may log notification failures
- caller modules may return success even if the notification failed
- delivery failures must remain observable through dispatch records

---

## Why This Matters

This is different from `Shipment`.

If shipment creation fails, fulfillment may be incomplete.
If notification sending fails, the commercial truth is still valid.

So `Notification` should be treated as:

- operationally important
- business-visible
- but not a transactional owner of the upstream action

---

## Recommended MVP Integration Pattern

The rest of the codebase currently uses:

- direct synchronous integration
- module APIs
- integration services
- manual compensation in selected flows

`Notification` should follow that same pattern during MVP.

Recommended phase 1 integration:

- `Order` calls `Notification` after successful `Place` and `Cancel`
- `Payment` calls `Notification` after successful `Authorize`, `Capture`, `Fail`, `Refund`
- `Shipment` calls `Notification` after successful `Create`, `Ship`, `Deliver`, `DeliveryException`

Do this **after** the source module has already saved its own state.

---

## Recommended Module API

For clarity, prefer explicit transactional methods over a fully generic "send anything" API.

Suggested contract shape:

- `SendOrderPlacedAsync`
- `SendOrderCancelledAsync`
- `SendPaymentAuthorizedAsync`
- `SendPaymentCapturedAsync`
- `SendPaymentFailedAsync`
- `SendPaymentRefundedAsync`
- `SendShipmentCreatedAsync`
- `SendShipmentShippedAsync`
- `SendShipmentDeliveredAsync`
- `SendShipmentDeliveryExceptionAsync`

Under the hood these can delegate to one common orchestration service.

This is more explicit than a weakly typed generic payload API and fits the repo style better.

---

## Commands

Suggested application commands:

- `CreateNotificationTemplate`
- `UpdateNotificationTemplate`
- `ActivateNotificationTemplate`
- `DeactivateNotificationTemplate`
- `SendOrderPlacedNotification`
- `SendOrderCancelledNotification`
- `SendPaymentCapturedNotification`
- `SendShipmentShippedNotification`
- `SendShipmentDeliveredNotification`

You do not need to implement every trigger on day one, but the model should allow it cleanly.

---

## Queries

Suggested queries:

- `GetNotificationDispatchById`
- `SearchNotificationDispatches`
- `GetNotificationTemplateById`
- `SearchNotificationTemplates`

Useful filter dimensions:

- store id
- channel
- trigger
- status
- entity type
- entity id
- created date range

This gives operators a real audit view.

---

## Suggested API Surface

Admin endpoints for phase 1:

- `GET /api/stores/me/notification-templates`
- `GET /api/stores/me/notification-templates/{templateId}`
- `POST /api/stores/me/notification-templates`
- `PUT /api/stores/me/notification-templates/{templateId}`
- `POST /api/stores/me/notification-templates/{templateId}/activate`
- `POST /api/stores/me/notification-templates/{templateId}/deactivate`
- `GET /api/stores/me/notifications`
- `GET /api/stores/me/notifications/{dispatchId}`

Do not expose generic public send endpoints to customers in phase 1.

---

## Persistence Recommendation

Suggested tables:

- `NotificationTemplates`
- `NotificationDispatches`
- `NotificationAttempts`

Important indexes:

- unique `(StoreId, Trigger, Channel, Locale)` for active template identity
- `(StoreId, Status, CreatedAtUtc)` for dispatch search
- `(StoreId, BusinessEntityType, BusinessEntityId)` for traceability
- `ProviderMessageId` for external reconciliation

---

## Failure Handling

For MVP:

- persist dispatch first
- attempt provider call
- persist success/failure result

If the provider call fails:

- mark dispatch as `Failed`
- save failure reason
- return or bubble an application exception depending on the upstream use case

Recommended upstream behavior:

- log warning
- do not compensate order/payment/shipment state

Later, this module can gain:

- retry jobs
- dead-letter handling
- outbox-based publishing
- scheduled resend workflows

---

## Future Event / Outbox Direction

Even if MVP uses synchronous calls, design the module so it can evolve cleanly.

Later direction:

- source modules publish integration events
- `Notification` consumes them through outbox/event flow
- failed notifications gain retry semantics
- multi-step communication workflows can use saga/process-manager patterns if needed

Good future event candidates:

- `OrderPlaced`
- `OrderCancelled`
- `PaymentCaptured`
- `PaymentFailed`
- `PaymentRefunded`
- `ShipmentShipped`
- `ShipmentDelivered`

For now, keep the boundaries event-friendly even if the implementation stays synchronous.

---

## Recommended Phase 1 Final Shape

If we want a professional but not overengineered MVP, the best version of this module is:

- transactional communication only
- email-first
- explicit templates
- persisted dispatch history
- direct synchronous invocation from upstream modules
- no rollback of commercial truth on notification failure
- future-ready boundaries for event/outbox migration

That gives the project a strong notification foundation without forcing campaign
infrastructure too early.
