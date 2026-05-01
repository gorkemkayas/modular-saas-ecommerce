# Modular SaaS E-Commerce Backend

A multi-tenant e-commerce backend built with ASP.NET Core using a modular monolith architecture.

This project is being developed as a portfolio-grade, industry-style backend with a strong focus on:

- Domain-Driven Design
- Clean Architecture
- CQRS with MediatR
- per-module boundaries
- explicit cross-module contracts
- PostgreSQL persistence with EF Core

The system integrates with an external `AuthService` for authentication and JWT-based identity.

## Current Scope

The project currently contains these modules:

- `Store`
- `Catalog`
- `Customer`
- `Pricing`
- `Order`
- `Inventory`
- `Payment`
- `Shipment`

These modules already model the core commerce flow:

- store and tenant context
- product catalog management
- customer profile and address management
- price list and price resolution
- order placement and cancellation
- inventory availability, reservation, release, and stock movement tracking
- payment lifecycle orchestration
- outbound shipment and tracking management

## Architecture

The codebase follows a modular monolith structure.

Each module is organized around:

- `Application`
- `Domain`
- `Infrastructure`
- optional `Contracts`
- module-specific tests

Main architectural decisions:

- business rules live in the domain model
- use cases are handled in the application layer
- EF Core and persistence concerns stay in infrastructure
- modules communicate through contracts and integration services instead of direct internal coupling
- read and write concerns are separated where appropriate

## Tech Stack

- ASP.NET Core
- C#
- MediatR
- Entity Framework Core
- PostgreSQL
- Serilog
- JWT Authentication

## Current Status

The backend is actively under development, but it is no longer just a planned structure.

Implemented today:

- multi-tenant request context support
- store management
- catalog management and storefront queries
- customer management
- pricing module with price lists and resolved pricing
- order module with order snapshots and lifecycle handling
- inventory module with reservation-based stock protection
- payment module with authorization, capture, cancel, refund, and order sync
- shipment module with package-level fulfillment and tracking events

Planned next core modules:

- `Notification`

These three modules are the remaining core pieces needed to complete the first full backend MVP.

## Progress Visuals

We are currently focused on Stage 1.

<img width="875" height="515" alt="currentStatus" src="https://github.com/user-attachments/assets/8ed8af28-dd9c-4151-8079-a597aa9102eb" />

Our next goal is to complete Stage 2.

<img width="1222" height="502" alt="aimedStatus" src="https://github.com/user-attachments/assets/6040709b-88ec-4fbe-9f4f-3c6e65550cd1" />

## Inventory Highlights

The `Inventory` module currently includes:

- inventory item ownership per store and sellable item
- on-hand, reserved, and available stock tracking
- reservation lifecycle management
- stock movement history
- reorder threshold support
- integration with the order flow

Order placement now performs real inventory availability checks and creates reservations instead of relying on a no-op placeholder.

## Project Structure

```text
src/
  BuildingBlocks/
  Host/
    ECommerce.API/
  Modules/
    Store/
    Catalog/
    Customer/
    Pricing/
    Order/
    Inventory/
    Payment/
    Shipment/
docs/
  OrderModuleDesign.md
  PricingModuleDesign.md
  ShipmentModuleDesign.md
```

## Running the API

The API host project is:

- `src/Host/ECommerce.API`

Development configuration is currently stored in:

- `src/Host/ECommerce.API/appsettings.Development.json`

To build the API:

```powershell
dotnet build src/Host/ECommerce.API/ECommerce.API.csproj
```

To run the API:

```powershell
dotnet run --project src/Host/ECommerce.API/ECommerce.API.csproj
```

## Documentation

Additional design notes are available in:

- [docs/OrderModuleDesign.md](docs/OrderModuleDesign.md)
- [docs/PricingModuleDesign.md](docs/PricingModuleDesign.md)
- [docs/ShipmentModuleDesign.md](docs/ShipmentModuleDesign.md)

## Roadmap

Short-term roadmap:

1. complete `Notification`
2. build the frontend MVP
3. expand with optional product modules such as `Cart`, `Promotion`, `Return`, and `Review`

## Notes

This repository is being developed as a graduation and portfolio project.

The goal is not just to make the system work, but to make the architecture, module boundaries, and implementation style reflect production-minded backend engineering decisions.

👀
<img width="1253" height="711" alt="image" src="https://github.com/user-attachments/assets/f1647c60-1310-4883-9ee1-4fad850fefa7" />
