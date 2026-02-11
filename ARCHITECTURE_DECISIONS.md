# Architecture & Design Decisions

This document outlines the architectural blueprint and core design decisions for the Clinic POS v1 system.

## Project Structure
```text
src/
├── backend/            # .NET 10 Web API (Thin Vertical Slice Architecture)
│   ├── Controllers/    # API Endpoints
│   ├── Data/           # EF Core Context & Seeding
│   ├── Models/         # Domain Entities
│   ├── Services/       # ICurrentTenant & Middleware
│   └── Events/         # RabbitMQ Message Contracts
├── backend.tests/      # Integration Tests (xUnit)
└── frontend/           # Next.js 14 App Router
    ├── src/app/        # Pages & Layouts
    └── src/lib/        # API Client & Shared Utilities
```

## Backend Architecture Overview
We follow a **Thin Vertical Slice** approach. Instead of complex layers (Clean/Onion), we group logic around domain entities (`Patients`, `Users`) to minimize boilerplate for a v1. Currently running on **.NET 10.0**.

## Tenant Isolation Strategy
**Goal**: Absolute data isolation at the DB level.

- **Option A: Table-per-tenant** (Separate DBs or Schemas). Excellent isolation but high operational overhead for many tenants.
- **Option B: Column-per-tenant** (Shared DB, `TenantId` column). Simplified scaling and reporting.
- **Chosen**: **Option B**.
- **Rationale**: Given the 90-minute timebox, Option B is faster to implement and scale. We enforce safety via **EF Core Global Query Filters**.

### Enforcement Design
1. **Middleware**: Extracts `tenant_id` from the JWT and injects it into a scoped `ICurrentTenant` service.
2. **Context**: `ClinicDbContext` automatically applies `Where(p => p.TenantId == tid)` to every query on `Patient`, `Branch`, and `AppUser`.
3. **Write Protection**: The API ignores any `TenantId` sent by the client, setting it server-side from the authenticated token context.

## Branch Relation Strategy
**Goal**: Define how patients interact with clinics.

- **Option A: Many-to-Many** (Patient can belong to many branches). More flexible but requires join tables and complex sync logic.
- **Option B: One-to-Many** (Patient belongs to 1 Tenant, has 1 Primary Branch). Very simple, covers 80% of v1 use cases.
- **Chosen**: **Option B**.
- **Rationale**: For v1, the primary concern is which branch a patient *registered* at. They can still visit others (handled via visit logs in v2), but the data ownership is cleanly mapped to a single "Primary Branch".

## Security & Auth Design
- **JWT Strategy**: Stateless tokens containing `email`, `role`, and `tenant_id`.
- **Policy-based Auth**: 
    - `CanViewPatients`: Admin, User, Viewer.
    - `CanCreatePatients`: Admin, User.
- **DB Uniqueness**: A composite unique index `(TenantId, PhoneNumber)` prevents accidental cross-tenant duplicates while allowing the same phone number to exist in *different* tenants.

## Caching & Messaging
- **Cache Strategy**: Tenant-scoped keys: `tenant:{tid}:patients:list:{branchId}`. Prevents data from one tenant being served to another if a cache key collision were to occur.
- **RabbitMQ**: Publishes `PatientCreated` events to a `patient-events` exchange. Minimal logic for high-performance scale.

## Integration Test Strategy
We use `WebApplicationFactory` to spin up a real test host.
1. **Tenant Isolation Test**: Verify that Tenant A cannot see Tenant B's data even if they know the ID.
2. **Uniqueness Test**: Verify that a duplicate phone number within the *same* tenant returns a `409 Conflict`.
3. **Invalidation Test**: Verify that creating a patient clears the related Redis cache.

## Checklist Verification
- [x] Tenant isolation enforced (Global Query Filters)
- [x] Duplicate phone blocked at DB level (Unique Index)
- [x] Viewer cannot create patient (Policy-based Auth)
- [x] Seeder works (Automatic on startup)
- [x] Docker works (Multi-container orchestration)
- [x] Tests pass (3/3 integration tests)
