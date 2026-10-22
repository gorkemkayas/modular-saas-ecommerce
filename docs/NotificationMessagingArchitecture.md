# Notification Messaging Architecture

This document describes the event-driven messaging pipeline currently implemented
between the `Order` and `Notification` modules. It covers transactional outbox
publishing, RabbitMQ delivery and retry topology, transactional Inbox persistence,
and asynchronous Inbox processing.

## End-to-End Flow

```mermaid
flowchart TD
    A[Order.Cancel] --> B[OrderCancelledDomainEvent]
    B --> C[Domain event handler]
    C --> D[OrderCancelledIntegrationEvent]
    D --> E[(Order Outbox)]

    E --> F[Order Outbox Background Service]
    F --> G[Order Outbox Processor]
    G --> H[Order RabbitMQ Publisher]
    H --> I[ecommerce.events Topic Exchange]

    I -->|order.order-cancelled.v1| J[Notification Main Queue]
    J --> K[Notification Integration Event Consumer]
    K --> L{Delivery valid?}

    L -->|No| M[Permanent failure]
    M --> N[Dead-letter Exchange]
    N --> O[Dead-letter Queue]

    L -->|Yes| P[Inbox Message Writer]
    P --> Q{Inbox write result}
    Q -->|Inserted or duplicate| R[Commit Inbox transaction]
    R --> S[ACK delivery]

    Q -->|Transient failure| T[Failure Router]
    T --> U[Retry Exchange]
    U --> V[Retry Queue]
    V -->|TTL expires| J
    U -->|Retries exhausted| N
```

The consumer follows one critical ordering guarantee:

```text
Persist the message in the Inbox
    -> commit the database transaction
    -> acknowledge the RabbitMQ delivery
```

RabbitMQ never receives a successful acknowledgement before the message is durably
stored in the Notification database.

## RabbitMQ Topology

```mermaid
flowchart LR
    EVENTS[ecommerce.events<br/>Topic Exchange]
    MAIN[notification.integration-events<br/>Main Queue]
    RETRY[notification.integration-events.retry<br/>Headers Exchange]

    R1[retry.1.queue<br/>5 seconds]
    R2[retry.2.queue<br/>30 seconds]
    R3[retry.3.queue<br/>2 minutes]
    R4[retry.4.queue<br/>10 minutes]

    DLX[notification.integration-events.dead-letter<br/>Fanout Exchange]
    DLQ[notification.integration-events.dead-letter.queue]

    EVENTS -->|order.order-cancelled.v1| MAIN

    RETRY -->|RetryAttempt = 1| R1
    RETRY -->|RetryAttempt = 2| R2
    RETRY -->|RetryAttempt = 3| R3
    RETRY -->|RetryAttempt = 4| R4

    R1 -->|TTL via default exchange| MAIN
    R2 -->|TTL via default exchange| MAIN
    R3 -->|TTL via default exchange| MAIN
    R4 -->|TTL via default exchange| MAIN

    DLX --> DLQ
```

Expired retry messages return directly to the Notification main queue through
RabbitMQ's default exchange. They do not return to the global event exchange. This
prevents other modules that already handled the event from receiving the retry.

## Consumer Delivery Flow

```mermaid
flowchart TD
    A[RabbitMQ delivery] --> B[Validate MessageId]
    B --> C[Validate EventType]
    C --> D[Validate content type and encoding]
    D --> E[Read retry metadata]
    E --> F[Decode UTF-8 body]
    F --> G[Validate JSON]
    G --> H[Create InboxMessage]

    H --> I[Create delivery scope]
    I --> J[InboxMessageWriter.WriteAsync]
    J --> K{Write result}

    K -->|Inserted| L[(Inbox row)]
    K -->|Duplicate| M[Keep existing row]
    K -->|Database failure| N[Route transient failure]

    L --> O[ACK]
    M --> O

    N --> P[Publish to Retry or DLQ]
    P --> Q{Publisher confirm received?}
    Q -->|Yes| R[ACK original delivery]
    Q -->|No| S[Wait recovery interval]
    S --> T[NACK and requeue]
```

Each delivery gets its own dependency-injection scope and `NotificationDbContext`.
This prevents concurrent deliveries from sharing a non-thread-safe EF Core context.

### ACK Failure After Inbox Commit

```mermaid
flowchart TD
    A[Inbox transaction committed] --> B[ACK attempted]
    B -->|ACK succeeds| C[Delivery complete]
    B -->|ACK fails| D[Delivery remains unacknowledged]
    D --> E[Broker redelivers after channel recovery]
    E --> F[Inbox unique MessageId constraint]
    F --> G[Duplicate write result]
    G --> H[ACK redelivery]
```

The unique Inbox `MessageId` provides idempotency when an ACK is lost after a
successful database commit.

## Broker Failure Classification

```mermaid
flowchart TD
    A[Consumer failure] --> B{Failure category}

    B -->|Invalid MessageId| C[Permanent]
    B -->|Missing EventType| C
    B -->|Invalid JSON| C
    B -->|Invalid content metadata| C
    B -->|Malformed retry metadata| C

    B -->|Database unavailable| D[Transient]
    B -->|Timeout| D
    B -->|Temporary persistence error| D

    C --> E[Dead-letter Exchange]
    D --> F{Retry attempt available?}
    F -->|Yes| G[Matching retry queue]
    F -->|No| E
```

## Retry Metadata

The failure publisher preserves the original message properties and adds these
headers:

| Header | Purpose |
| --- | --- |
| `RetryAttempt` | Current RabbitMQ retry attempt |
| `OriginalExchange` | Exchange that received the original delivery |
| `OriginalRoutingKey` | Original integration-event routing key |
| `FirstFailedAtUnixTime` | UTC timestamp of the first failure |

```mermaid
flowchart LR
    A[BasicProperties.Headers]
    --> B[RabbitMqRetryMetadataCodec.Read]
    B --> C[RabbitMqRetryMetadata]
    C --> D[Increment RetryAttempt]
    D --> E[RabbitMqRetryMetadataCodec.Write]
    E --> F[Republished message headers]
```

Malformed or incomplete retry metadata is replaced with safe metadata before the
delivery is moved to the dead-letter queue. This prevents poison-message loops.

## Inbox Processing

The RabbitMQ consumer only validates and persists deliveries. Business processing
is performed separately by the Inbox background service.

```mermaid
flowchart TD
    A[Notification Inbox Background Service]
    --> B[Notification Inbox Processor]
    B --> C[Claim pending Inbox records]
    C --> D[Assign ClaimToken and ClaimedUntilUtc]
    D --> E[Commit short claim transaction]

    E --> F[Create one scope per claimed message]
    F --> G[Begin processing transaction]
    G --> H[Resolve handler by EventType]

    H -->|order.order-cancelled.v1| I[OrderCancelled Inbox Handler]
    H -->|Unknown event| J[Unsupported event failure]

    I --> K[Deserialize payload]
    K --> L[Create NotificationDispatch]
    L --> M[Mark Inbox as Processed]
    M --> N[SaveChanges]
    N --> O[Commit transaction]
```

Creating the `NotificationDispatch` and marking the Inbox record as `Processed`
happen in the same transaction. If either operation fails, both are rolled back.

## Inbox State Transitions

```mermaid
stateDiagram-v2
    [*] --> Pending: Consumer persists delivery
    Pending --> Processing: Worker claims message
    Processing --> Processed: Dispatch created and transaction committed
    Processing --> Pending: Transient failure and attempts remain
    Processing --> Failed: Permanent failure
    Processing --> Failed: Maximum attempts reached
    Processing --> Pending: Claim lease expires
```

When transient processing fails, the processor calculates `NextAttemptAtUtc` using
exponential backoff with jitter. A new scope and DbContext persist the failure
result after the processing transaction has rolled back.

## Handler Resolution

```mermaid
flowchart LR
    A[ClaimedInboxMessage.EventType]
    --> B[NotificationInboxMessageHandlerResolver]
    B --> C[Registered Inbox handlers]
    C --> D[OrderCancelledInboxMessageHandler]
    C --> E[Future event handlers]
    B -->|No match| F[UnsupportedNotificationInboxEventException]
```

Adding another integration event requires a new
`INotificationInboxMessageHandler` implementation and DI registration. The Inbox
processor does not need event-specific changes.

## Retry Boundaries

The architecture has separate retry mechanisms for separate reliability boundaries:

| Mechanism | Boundary | Handles |
| --- | --- | --- |
| RabbitMQ retry queues | Broker to Inbox | Failure to persist a delivery in the Inbox |
| Inbox database retry | Inbox to dispatch | Failure to transform an Inbox message into a dispatch |
| Dispatch retry (next phase) | Dispatch to provider | Failure to send through the email provider |

```mermaid
flowchart LR
    A[RabbitMQ Message]
    -->|Broker retry| B[(Inbox)]
    B -->|Inbox processing retry| C[(NotificationDispatch)]
    C -->|Dispatch retry| D[Email Provider]
```

## Component Map

| Responsibility | Main component |
| --- | --- |
| Consume RabbitMQ deliveries | `NotificationIntegrationEventConsumer` |
| Declare queue, retry, and DLQ topology | `NotificationIntegrationEventTopology` |
| Select retry tier or DLQ | `NotificationIntegrationEventFailureRouter` |
| Publish failed deliveries with confirms | `RabbitMqFailureMessagePublisher<TPublisherIdentity>` |
| Encode and decode retry headers | `RabbitMqRetryMetadataCodec` |
| Persist idempotent Inbox rows | `InboxMessageWriter` |
| Claim and update Inbox records | `NotificationInboxStore` |
| Schedule Inbox processing | `NotificationInboxBackgroundService` |
| Orchestrate claimed messages | `NotificationInboxProcessor` |
| Resolve event-specific behavior | `NotificationInboxMessageHandlerResolver` |
| Handle order cancellation | `OrderCancelledInboxMessageHandler` |

## Current Boundary

The implemented pipeline currently ends after a `NotificationDispatch` is created:

```text
Domain Event
    -> Transactional Outbox
    -> RabbitMQ
    -> Broker retry / DLQ
    -> Transactional Inbox
    -> Claim-based Inbox processing
    -> Event-specific handler
    -> NotificationDispatch
```

The next phase will add claim-based `NotificationDispatch` delivery, email-provider
integration, retry scheduling, and terminal failure handling.
