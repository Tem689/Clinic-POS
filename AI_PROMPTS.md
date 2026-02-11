# AI Prompts & Decision Log

## Initial Prompt
> "Build a Clinic POS v1 thin slice system with .NET , Next.js, Postgres, Redis, and RabbitMQ. Focus on tenant safety. [Full Spec Provided]"

## Iterations & Prompts

### 1. Infrastructure Scaffolding
- **Prompt**: "Create a docker-compose.yml with Postgres, Redis, and RabbitMQ. Create a .NET 10 Web API in src/backend and a Next.js app in src/frontend."
- **Iteration**: Modified `docker-compose.yml` host ports (5433, 6380, etc.) to resolve local machine conflicts.

### 2. Multi-Tenancy Design
- **Prompt**: "Implement a tenant isolation strategy using JWT claims. Use EF Core Global Query Filters for Patients, Branches, and Users. The TenantId should be set in a scoped service from a middleware."
- **Decision**: Used `ICurrentTenant` service + `TenantMiddleware`. This ensures "safe by default" reads.

### 3. Caching & Messaging
- **Prompt**: "Add Redis caching for the List Patients endpoint. Invalidate the cache when a new patient is created. Publish a PatientCreated event to RabbitMQ."
- **Outcome**: Implemented tenant-scoped cache keys `tenant:{tid}:...` to prevent data leakage in cache.

### 5. Appointments & Concurrency (Section C)
- **Prompt**: "Implement Section C: Create Appointment with TenantId, BranchId, and PatientId. Ensure concurrency safety via DB unique constraint. Publish AppointmentCreated event to RabbitMQ."
- **Decision**: Added composite unique index `(PatientId, StartAt, BranchId)` to ensure no double bookings. Added a manual pre-check in the controller for `InMemoryDatabase` compatibility during tests.

## Accepted vs Rejected Outputs
- **Accepted**: Scoped service for `TenantId` (Clean & Testable).
- **Accepted**: Index-based uniqueness constraint (Robust under concurrency).
- **Accepted**: Manual pre-check for duplicates (Necessary for xUnit InMemory provider).
- **Rejected**: Auto-generating a full CRUD suite for all entities. I restricted it to `Patients`, `Users`, and `Appointments` to meet the "thin slice" timebox.

## Correctness Validation
- **Automated**: Integrated 4 backend testing scenarios (Tenant Leak, Duplicate Phone, Cache Invalidation, Concurrent Appointment).
- **Manual**: Verified Docker build and .NET 10 compilation within the container.
- **Tools**: Used `dotnet test` and `docker compose build`.
