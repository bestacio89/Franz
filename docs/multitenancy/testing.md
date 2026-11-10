# Testing multi-tenant services

A passing happy-path test is not evidence of tenant isolation. Test the security boundary, persistence behavior, routing, and lifecycle under both valid and adversarial conditions.

## Tenant fixtures

Use at least two tenants with distinct IDs and distinct owned records. Include a verified domain for one tenant and, where relevant, a second domain belonging to the same tenant. Include an inactive tenant and an unknown tenant ID.

Never use the same tenant fixture for both sides of a cross-tenant test; that can make isolation defects invisible.

## HTTP tests

Verify:

- A valid tenant selector resolves to the expected active tenant.
- Unknown, inactive, malformed, and ambiguous selectors are rejected.
- An authenticated user who belongs to the tenant is authorized.
- An authenticated user who does not belong to the tenant is rejected.
- A missing authorizer fails closed when authenticated authorization is required.
- A supplied tenant header/claim alone does not bypass membership authorization.
- A domain not associated with the resolved tenant is rejected.
- Excluded health paths bypass tenant resolution but expose no tenant data and remain independently secured.
- The execution context is cleared after normal completion, exception, and cancellation.

## Messaging tests

Verify:

- Exactly one valid tenant header is required when tenant scope is enabled.
- Unknown/inactive tenants and invalid domains are rejected.
- Producer authorization is checked independently of message headers.
- Unauthorized producers cannot publish for another tenant.
- A new DI scope and execution context are created for each delivery.
- The scope is cleared after success, handler exception, retry, and cancellation.
- Tenant context is propagated to outgoing messages only from a validated execution context.
- Dead-letter and retry paths preserve metadata without bypassing revalidation.

## Persistence tests

For `SharedDatabase`, seed records for tenant A and tenant B, then assert:

- Tenant A cannot query tenant B's rows through normal EF queries.
- Tenant A cannot update or delete tenant B's row by attaching it directly.
- New tenant-owned rows are stamped with the current tenant.
- A caller cannot reassign `TenantId`.
- Global entities remain visible only where intended.
- Every persisted `IEntity<TKey>` has exactly one ownership marker when shared-database multi-tenancy is enabled.
- Tenant-aware unique constraints and indexes behave as designed.

For `TenantDatabase`, assert the resolver selects the expected tenant-specific database name and fails when context or token is missing. Also test the actual provider connection against two isolated databases.

For `DomainDatabase`, assert the resolver uses only a verified domain ID and fails when domain context is absent. Test that a tenant ID is never used as a fallback.

## Cache tests

Verify identical request keys under two tenants produce different cache entries. Verify a missing tenant context throws when tenant-scoped keys are enabled. Verify global cache entries are intentionally global and do not contain tenant-owned values.

Test invalidation after tenant metadata and domain changes, including multi-instance propagation if the deployment uses multiple replicas.

## Background-job tests

Verify every job execution creates a new scope, revalidates tenant status and job producer authority, and cannot change tenant identity through payload manipulation. Test concurrent jobs for different tenants and retries after tenant suspension.

## Provider-specific qualification

Run integration tests against the real database engine/version used in production. Test EF query translation, transactions, concurrency, migrations, and raw SQL separately. For Cosmos, test partition-key behavior and native repository filters. For MongoDB, test tenant filters on every read and mutation path.

Do not infer that query filters protect raw SQL, stored procedures, provider-native bulk operations, or external clients. Add database-level row security where available and prohibit unreviewed bypass paths.

## Release gate

A service should not be considered tenant-ready until:

- All positive and negative boundary tests pass.
- Cross-tenant reads and writes are rejected.
- Cache and message context are isolated.
- Background jobs establish their own validated scope.
- Provider integration tests pass against the production engine/version.
- Migration, provisioning, and tenant revocation procedures are exercised.
- Architecture tests prevent accidental dependency or registration drift.
