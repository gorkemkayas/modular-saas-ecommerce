# AGENTS.md

## Project Overview
This repository is a modular monolith, multi-tenant e-commerce backend built with ASP.NET Core.

Architecture and design principles:
- Modular Monolith
- Domain-Driven Design (DDD)
- Clean Architecture
- CQRS
- MediatR
- EF Core
- PostgreSQL

This codebase is intended to be portfolio-grade and industry-style.
Prefer maintainable, explicit, and professional code over shortcuts.

---

## Core Architecture Rules

### Module Boundaries
- Respect module boundaries at all times.
- Do not create tight coupling between modules.
- Do not directly reach into another module’s internals unless an existing integration pattern already allows it.
- Prefer module APIs, contracts, application abstractions, or integration/event-based communication when needed.

### Layer Boundaries
Follow the existing architectural layering:

- **Domain** → business rules, invariants, entities, value objects, domain services, repository abstractions
- **Application** → use cases, commands, queries, handlers, orchestration, validation
- **Infrastructure** → EF Core, persistence, external services, repository implementations, integrations
- **API / Host** → controllers, HTTP endpoints, dependency injection wiring, middleware, composition root

Do not move responsibilities to the wrong layer.

---

## Domain Rules
- Keep Domain pure.
- Do not introduce infrastructure concerns into Domain.
- Do not inject EF Core, database, HTTP, or framework-specific logic into Domain entities.
- Business invariants should be protected inside aggregates and domain methods.
- Child entities should be controlled through their aggregate root.
- Prefer meaningful Value Objects where they improve correctness and clarity.
- Avoid anemic domain models.

---

## Application Rules
- Application layer should orchestrate use cases, not become a dumping ground for business logic.
- Use CQRS consistently where it already exists.
- Commands and Queries must stay separated.
- Handlers should remain focused and cohesive.
- Validation should be explicit.
- Uniqueness checks and repository-backed existence checks may be handled in Application when they require persistence access.

---

## Infrastructure Rules
- EF Core configurations belong in Infrastructure.
- Repository implementations belong in Infrastructure.
- Keep persistence concerns out of Domain and Application unless there is an intentional abstraction already in place.
- Follow existing DbContext and module registration patterns.

---

## Coding Style
- Prefer explicit, readable code over clever abstractions.
- Reuse existing project patterns before introducing new ones.
- Do not introduce unnecessary abstractions, base classes, helpers, or utility layers.
- Do not introduce new libraries unless there is a clear need.
- Avoid overengineering.
- Keep files cohesive and responsibilities clear.
- Use clear naming and consistent conventions.

---

## Mapping
- Prefer explicit/manual mapping unless the project already uses a mapping approach in that area.
- Do not introduce AutoMapper or similar tools unless explicitly requested.

---

## Refactoring Rules
- Do not refactor unrelated code.
- Do not rename or move files unless necessary for the requested task.
- Keep changes scoped to the requested feature or bug.
- If you notice architectural issues outside the requested scope, mention them briefly instead of silently refactoring large areas.

---

## Testing and Verification
Before finishing a task, when possible:

1. Build the relevant project or solution
2. Run relevant tests if available
3. Check for obvious compile/runtime issues
4. Summarize clearly:
   - which files changed
   - why they changed
   - any assumptions made

---

## Working Style
When implementing a task:
1. First understand the local module and existing conventions
2. Follow existing structure before inventing a new one
3. Keep the solution simple and aligned with the current architecture
4. Prefer production-appropriate code over demo-style shortcuts

If the task is ambiguous, prefer the most conservative architecture-aligned solution.