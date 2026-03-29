# AGENTS.md

## Module Purpose
The Catalog module is responsible for product catalog management.

It includes concepts such as:
- Products
- Product Variants
- Categories
- Brands
- Product Attributes / Attribute Definitions
- Product Media
- Product Organization and Catalog Structure

This module should model catalog behavior in a domain-focused and maintainable way.

---

## Architectural Intent
This module follows:
- DDD
- Clean Architecture
- CQRS
- Explicit use case design

Prefer strong domain modeling over CRUD-style design when business rules exist.

---

## Domain Modeling Rules

### Aggregate Design
- Treat aggregate boundaries seriously.
- Protect invariants inside aggregate roots.
- Child entities must be managed through the aggregate root.
- Do not allow external code to mutate internal collections directly.
- Prefer encapsulation over convenience.

### Important Modeling Preference
In this module, not every concept should be a primitive type.

Prefer:
- Value Objects for meaningful concepts
- Child entities when lifecycle and identity matter
- Explicit domain modeling when it improves correctness and clarity

Examples include concepts such as:
- SKU
- Slug / Code-like identifiers
- Product media structures
- Brand-like domain concepts
- Attribute-related structures

Do not flatten everything into strings and primitive properties unless there is a good reason.

### Collections
- Internal mutable collections should remain private.
- Public exposure should be read-only.
- Collection mutations should happen through domain methods.
- Aggregate roots should control child entity lifecycle.

### Invariants
Protect important business rules inside the domain model whenever possible.

Examples:
- invalid product state transitions
- duplicate child elements inside an aggregate
- invalid attribute combinations
- invalid SKU or code structures
- invalid variant definitions

Do not rely only on controllers or handlers to protect domain consistency.

---

## Application Layer Rules

### Use Cases
Organize Application around use cases / features.

Typical structure may include:
- Commands
- Queries
- DTOs / Responses
- Validators
- Handlers

Prefer feature-oriented organization over generic technical dumping.

### CQRS
- Keep Commands and Queries separate.
- Handlers should focus on one use case.
- Avoid bloated handlers.
- Handlers should orchestrate, not contain all domain intelligence.

### Validation
- Input validation should be explicit.
- Use validators where appropriate.
- Domain invariants should still remain protected in the domain model.

### Persistence-backed Checks
Checks such as:
- uniqueness
- existence
- cross-aggregate lookup

may be handled in the Application layer when they require repository access.

Do not force repository-backed checks into entities when it harms domain purity.

---

## Infrastructure Rules
- EF Core configurations belong in Infrastructure.
- Respect existing mappings and persistence conventions.
- Preserve encapsulation when configuring entities and child collections.
- Do not weaken the domain model just to make EF mapping easier.

---

## API / Endpoint Guidance
When exposing Catalog use cases through endpoints:

- Keep endpoints thin
- Push use case logic into Application
- Do not place business logic inside controllers
- Use request/response contracts clearly
- Follow existing route and naming conventions in the module

---

## Coding Preferences for This Module
- Prefer explicit code over magic abstractions
- Keep behavior close to the model it belongs to
- Do not introduce unnecessary generic repositories or generic services
- Avoid “enterprise-looking” abstractions that add little value
- Favor clarity and maintainability

---

## Before Completing Work in This Module
Before finalizing a task in Catalog:

1. Check existing patterns in this module first
2. Keep changes aligned with current aggregate design
3. Avoid breaking encapsulation
4. Clearly explain:
   - what changed
   - why it changed
   - whether any domain rule was introduced or modified