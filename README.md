<<<<<<< HEAD
# Clinic-POS
Test
=======
# Clinic POS v1 - Thin Slice

A multi-tenant, multi-branch Clinic POS system built with .NET 9, Next.js, PostgreSQL, Redis, and RabbitMQ.

## Architecture Overview

- **Backend**: ASP.NET Core Web API (.NET 10).
- **Frontend**: Next.js App Router (TypeScript).
- **Architecture**: [Detailed Design Decisions](file: Clinic%20POS/ARCHITECTURE_DECISIONS.md)
- **Data Isolation**: EF Core Global Query Filters + `TenantId` derived from JWT claims.
- **Caching**: Redis for Patient List, invalidated on create.
- **Messaging**: MassTransit with RabbitMQ (Event publishing enabled).
- **Auth**: JWT-based authentication with simulated login for Admin, User, and Viewer roles.
- **Market**: Thailand-first, Global-ready (B2B Multi-tenant).

## Tenant Isolation Strategy (E2)

1. **Derivation**: The `TenantId` is extracted from the `tenant_id` claim in the JWT by the `TenantMiddleware`. This ID is stored in a scoped `ICurrentTenant` service.
2. **Enforcement**: In `ClinicDbContext`, a Global Query Filter is applied to all tenant-scoped entities (`Patient`, `Branch`, `AppUser`).
3. **Prevention**: The filter ensures that any query (including those written by mistake without a `.Where`) will automatically include the `TenantId` filter in the SQL.
4. **Writes**: The `TenantId` is set server-side in the `PatientsController` from the `ICurrentTenant` context, ignoring any tenant ID provided by the client.

## Caching Strategy (Section D)

1. **GET Caching**: The `GetPatients` endpoint is cached in Redis using a tenant-scoped key: `tenant:{tid}:patients:list:{branchId|all}`.
2. **Invalidation**: When a new patient is created via `CreatePatient`, the related cache keys for that tenant are invalidated (removed) to ensure data consistency.

## Branch Relation Strategy (A3)

- **Design**: We use a `Many-to-One` relationship where a Patient belongs to one Tenant and has one **Primary Branch**.
- **Rationale**: For a "v1 thin slice", this simplifies the data model while allowing patients to be "associated" with a specific branch for operational purposes (e.g., initial registration branch). Expanding to a `Many-to-Many` (Patient <-> Branch) would require a join table and more complex filtering, which was avoided for the 90-minute timebox.

## One-Command Run

Run the entire stack (Postgres, Redis, RabbitMQ, Backend, Frontend):

```bash
docker compose up --build
```

- **Migrations**: Applied automatically on backend startup.
- **Seeder**: Runs automatically on startup if the database is empty.

### Seeded Users
| Role | Email | Password |
| :--- | :--- | :--- |
| Admin | admin@clinic.com | password |
| User | user@clinic.com | password |
| Viewer | viewer@clinic.com | password |

## Ports & Access
| Service | Host Port | Container Port |
| :--- | :--- | :--- |
| **Frontend** | [localhost:3001](http://localhost:3001) | 3000 |
| **Backend** | [localhost:8081](http://localhost:8081) | 8080 |
| **Postgres** | 5433 | 5432 |
| **Redis** | 6380 | 6379 |
| **RabbitMQ** | 5673 / 15673 | 5672 / 15672 |

## API Examples (curl)

### Login
```bash
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@clinic.com", "password":"password"}'
```

### List Patients
```bash
curl -X GET http://localhost:8081/api/patients \
  -H "Authorization: Bearer <TOKEN>"
```

### Create User (B3)
```bash
curl -X POST http://localhost:8081/api/users \
  -H "Authorization: Bearer <ADMIN_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"email":"newuser@clinic.com", "role":"User", "tenantId":1, "branchIds":[1, 2]}'
```

## Running Tests

### Backend Unit/Integration Tests
```bash
cd src/backend.tests
dotnet test
```
*Note: Includes 3 integration tests covering Tenant Isolation, Unique Phone Number, and Cache Invalidation.*

## Trade-offs & Assumptions
- **.NET Version**: .NET 10.0 (Latest).
- **Soft Deletes**: Not implemented in v1 to keep complexity minimal.
>>>>>>> 929cdc5 (Initial commit: Clinic POS v1 with .NET 10 upgrade and UI Revamp)
