# Multi-tenancy integration

The Franz service template enables tenant isolation by default. It uses a shared relational database with explicit entity ownership classification.

## API host

The API binds `MultiTenancy:Http` to `TenantResolutionOptions` and requires both a resolved tenant and authenticated tenant membership. The configured `tenant_id` claim authorizer compares the authenticated principal's claim with the resolved tenant ID.

A consuming service must register:
- An `ITenantStore` backed by its authoritative tenant registry.
- At least one `ITenantResolver` (for example, a trusted host, route, or header resolver).
- Authentication and authorization configuration that issues the configured tenant claim.

Do not trust a client-supplied tenant header by itself. The resolver must map the request to a known active tenant, and the authorizer must verify that the authenticated principal is a member of that tenant. The API intentionally fails startup when required tenant-resolution services are absent.

## Consumer host

The Consumer binds `Messaging:Tenant:RequireTenantScope` to `TenantMessagingOptions`. Tenant scope is required for inbound message handling.

A consuming service must register:
- An authoritative `ITenantStore`.
- An `ITenantMessageAuthorizer` that validates the broker/transport producer identity for the resolved tenant.
- An optional `ITenantValidator` when additional tenant policy checks are required.

The framework validates tenant existence and active status, invokes the configured validator, authorizes the producer, and validates any supplied domain before establishing the execution context. Never treat an arbitrary payload field or unverified message header as proof of tenant membership. Producers must propagate tenant context through Franz messaging identity/context headers. The hosted consumer fails before listening when the required tenant initializer cannot be resolved.

## Persistence

`Database:IsolationMode` is explicitly set to `SharedDatabase`. Every persisted `IEntity<TKey>` must implement exactly one of:
- `ITenantEntity` for tenant-owned data (the sample `Book` and `Member` entities use this).
- `IGlobalEntity` for intentionally shared data.

Tenant-owned rows receive the current validated tenant ID on insert. Query filters isolate reads; update/delete validation prevents cross-tenant writes. The Development seeder enumerates active tenants and executes each seeder in a fresh scope initialized for that tenant; sample seeding is idempotent and uses database-generated keys. Add the `TenantId` column and tenant-aware indexes to existing database schemas through provider-specific EF Core migrations before deploying this template against an existing database.

## Cache

The template registers `AddTenantScopedCacheKeys` after the Redis provider. Cache keys are prefixed with the validated tenant ID to prevent cross-tenant cache collisions. Keep cache access within the tenant execution scope.

## Provider selection and secrets

Database provider selection remains configuration-driven. Keep credentials and production connection strings in environment variables or a secret provider; committed appsettings files contain local development placeholders only.
