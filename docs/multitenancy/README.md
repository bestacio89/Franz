# Multi-tenancy guide

This guide describes how to use the multi-tenancy capabilities available in Franz.Common 3.0.0 and how to adapt the Franz service template to a concrete service.

The template starts with **tenant-required HTTP and messaging boundaries** and **shared relational database isolation**. It is a secure starting profile, not a requirement that every service use the same storage topology. Select the database isolation mode deliberately and implement the application-owned tenant registry, resolution, authorization, provisioning, and operations.

## Start here

| Concern | Guide |
|---|---|
| Concepts, trust boundaries, and service responsibilities | [Fundamentals](fundamentals.md) |
| Shared, tenant-per-database, and domain-per-database routing | [Database isolation modes](database-isolation.md) |
| HTTP tenant resolution and membership authorization | [HTTP integration](http.md) |
| Kafka/RabbitMQ message context and producer authorization | [Messaging integration](messaging.md) |
| gRPC client/server propagation and authorization | [gRPC integration](grpc.md) |
| Tenant-safe cache keys and tenant registry caching | [Caching](caching.md) |
| Background jobs and scheduled work | [Background jobs](background-jobs.md) |
| Database provisioning, migrations, credentials, and lifecycle | [Provisioning and migrations](provisioning-migrations.md) |
| Test strategy and isolation verification | [Testing](testing.md) |
| Relational, Cosmos DB, and MongoDB implementation differences | [Provider guidance](provider-guidance.md) |

## Template baseline

The API configuration enables tenant resolution and authenticated membership authorization. The Consumer requires a validated tenant execution scope for message processing. Both hosts use:

```json
{
  "Database": {
    "IsolationMode": "SharedDatabase",
    "DatabaseName": "franz_dev"
  }
}
```

The API uses `MultiTenancy:Http` and `MultiTenancy:Claims`. The Consumer uses `Messaging:Tenant:RequireTenantScope`. The exact configuration is in:

- `sources/back/Franz.API/appsettings.json`
- `sources/back/Franz.Consumer/appsettings.json`

The sample tenant store, resolver, request authorizer, and message authorizer are intentionally application-owned. A new service must implement and register these contracts against its authoritative identity and tenant registry before the relevant host can start.

## Important distinction

There are three database target-selection modes in `DatabaseIsolationMode`:

- `SharedDatabase`
- `TenantDatabase`
- `DomainDatabase`

These are **database routing policies**, not three different HTTP or messaging protocols. HTTP, messaging, and gRPC establish validated execution identity. Persistence consumes that identity to apply query/write isolation and, when configured, select a database target.

Do not treat database routing as database provisioning, and do not treat tenant resolution as authorization.

## Recommended adoption sequence

1. Define tenant and domain lifecycle, identity ownership, and membership rules.
2. Implement the authoritative `ITenantStore` and the boundary-specific resolver/authorizer contracts.
3. Keep the template's fail-closed HTTP and Consumer settings enabled.
4. Choose a database isolation mode and implement its provisioning and migration lifecycle.
5. Verify cache, messaging, gRPC, background-job, and administrative paths.
6. Run the cross-tenant negative tests in [Testing](testing.md) before deployment.

For the package-level contracts, consult the Franz.Common 3.0.0 documentation for `Franz.Common.MultiTenancy`, `Franz.Common.Http.MultiTenancy`, `Franz.Common.Messaging.MultiTenancy`, and the relevant persistence adapter.
