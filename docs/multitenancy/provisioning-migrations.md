# Provisioning, migrations, and tenant lifecycle

Database routing and tenant lifecycle are separate responsibilities. Franz.Common resolves the effective database name for a scoped execution. The application and its deployment platform own provisioning, credentials, schema lifecycle, capacity, and deprovisioning.

## Tenant lifecycle

Define explicit lifecycle states and allowed transitions for the service, for example:

- Pending provisioning.
- Active.
- Suspended.
- Deprovisioning.
- Retired.

Only active tenants should receive normal tenant execution contexts. The tenant registry is authoritative; status transitions must be reflected consistently across HTTP, messaging, gRPC, background jobs, and administrative operations.

Provisioning should be idempotent and observable. Record which steps completed so a failed workflow can safely resume or compensate.

## SharedDatabase lifecycle

For shared tables:

1. Apply schema migrations through the service deployment pipeline.
2. Add the tenant ownership column to tenant-owned tables.
3. Backfill existing records using an audited mapping from legacy ownership to tenant IDs.
4. Add tenant-aware indexes and constraints.
5. Verify query filters, write validation, unique constraints, and raw SQL access paths.
6. Enable tenant-required boundaries only after registry and authorization services are ready.

For uniqueness that is tenant-scoped, include `TenantId` in the unique index/constraint. A globally unique business key should remain globally unique only when that is the intended domain invariant.

## TenantDatabase lifecycle

For `TenantDatabase`, the application must:

- Create the tenant database and apply the correct schema version.
- Resolve database credentials through a secure secret/credential provider.
- Ensure the runtime identity can access only the databases it requires.
- Configure connection pooling and resource limits for many databases.
- Track schema version per tenant database.
- Handle partial provisioning, retries, and cleanup.
- Define backup, restore, retention, and deletion policies per tenant.

The `{dbName}` token only selects a database name. It does not create the database or provision credentials. Avoid embedding credentials or tenant-supplied strings in database names. The resolver uses the validated tenant GUID, not a display name.

## DomainDatabase lifecycle

For `DomainDatabase`, the same operational responsibilities apply, keyed by the verified domain identity. Domain-to-tenant association changes are security-sensitive: update the authoritative registry, invalidate cached metadata, and ensure the domain context is revalidated before routing.

If a domain moves between tenants, define whether its database is migrated, reassigned, or recreated. Do not let a DNS change alone transfer data ownership.

## Migration orchestration

Do not run schema migrations independently from every API or consumer replica at startup. Use a controlled deployment job or migration process with:

- A single-writer/locking strategy.
- Explicit target selection.
- A recorded migration version.
- Retry and failure reporting.
- Roll-forward/rollback policy.
- Compatibility checks between old and new application versions.

For tenant-per-database deployments, the migration orchestrator must enumerate eligible tenant databases from the authoritative registry, resolve credentials safely, and migrate them in bounded batches. It must not assume that a tenant database can be created by opening a DbContext.

The template runs development migrations/seeders only in Development. Production schema changes belong to deployment/orchestration.

## Credentials and connection pools

Keep connection strings, database passwords, Cosmos keys, and Mongo credentials in a secret manager or workload identity integration. Do not commit secrets to appsettings files or log them.

Database-per-tenant can create a large number of connection pools. Model pool count, per-pool limits, idle timeout, database connection quotas, and aggregate service concurrency before selecting the topology. A correct routing design can still exhaust the database platform if pool growth is unbounded.

## Deprovisioning and data lifecycle

Define retention, export, legal hold, deletion, and restore behavior before automating tenant deletion. Make deprovisioning auditable and idempotent. Do not immediately drop tenant data on a transient status change or failed billing/payment workflow without an explicit business/legal policy.

## Deployment checklist

- [ ] Tenant registry is authoritative and backed up.
- [ ] Provisioning is idempotent and observable.
- [ ] Credentials are externalized and least-privileged.
- [ ] Migrations are centrally orchestrated.
- [ ] Per-tenant schema versions are tracked where needed.
- [ ] Pool/resource limits are modelled.
- [ ] Backup/restore and tenant export have been tested.
- [ ] Tenant suspension and revocation propagate to all execution boundaries.
- [ ] Deprovisioning and retention policies are documented.
