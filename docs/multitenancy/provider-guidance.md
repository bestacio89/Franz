# Persistence provider guidance

Multi-tenancy behavior is not identical across storage engines. The relational EF adapters, Cosmos DB integration, and MongoDB integration have different routing, partitioning, and repository semantics.

## Relational EF Core providers

Franz.Common 3.0.0 has provider adapters for PostgreSQL, MariaDB, SQL Server, and Oracle. They share `DatabaseOptions` and `DatabaseTargetResolver` for database target selection.

For relational providers:

- `SharedDatabase`, `TenantDatabase`, and `DomainDatabase` are database-name routing modes.
- The DbContext must receive the validated execution accessor and relevant options for tenant-aware model behavior.
- Shared-table tenant-owned entities use `ITenantEntity` and a mapped `TenantId`; intentionally shared entities use `IGlobalEntity`.
- Query filters and tracked-write enforcement are framework behavior, but do not secure arbitrary raw SQL.
- Verify provider translation, transaction semantics, unique indexes, concurrency, and bulk-operation support against the exact engine and package version.

The Franz.Common multi-tenancy qualification notes distinguish provider routing coverage from full persistence isolation coverage. Do not interpret a successful database-name resolver test as proof that all provider-specific persistence operations are tenant-safe. PostgreSQL has real-engine coverage in the documented qualification matrix; additional engine qualification is required for other relational providers.

### Raw SQL and database security

EF global query filters do not rewrite arbitrary SQL. For shared-table tenancy, consider database row-level security where supported. Bind the tenant identity from a trusted execution context using safe connection/session initialization and reset it before pooled connections are returned. Test connection reuse across tenants. If database-level enforcement is not configured, prohibit unreviewed raw SQL against tenant-owned tables.

## Azure Cosmos DB

Cosmos DB is a separate provider model; it is not one of the relational database-name adapters.

The EF Cosmos integration can compose tenant filters when multi-tenancy is enabled and the context receives the validated tenant execution accessor. Native tenant-partitioned documents use `ICosmosTenantDocument` and the tenant repository registration:

```csharp
builder.Services.AddCosmosTenantRepository<CustomerDocument>("customers");
```

The target container must use `/tenantId` as its partition-key path. The repository obtains the validated tenant context from the current scope, scopes operations to that tenant, stamps empty tenant IDs on insert, and rejects cross-tenant ownership.

These are distinct persistence paths:

- EF Core entities follow the EF context/model behavior.
- Native Cosmos documents follow the Cosmos tenant repository and container partition-key contract.

Do not assume that relational `TenantDatabase` or `DomainDatabase` name replacement configures Cosmos containers or partition keys. Design partitioning, throughput, indexing, consistency, and container lifecycle explicitly. Test the actual Cosmos account/emulator behavior used by the service.

## MongoDB

Franz.Common.MongoDB is a native MongoDB integration, not an EF Core provider. It exposes tenant-aware repositories for documents implementing `IMongoTenantEntity`.

```csharp
builder.Services.AddMongoTenantRepository<CustomerDocument>("customers");
```

The tenant repository requires a scoped `ITenantExecutionContextAccessor` and includes the validated tenant ID in read/mutation filters. Inserts stamp an empty `TenantId` from the current context and reject cross-tenant ownership.

Tenant-aware aggregate repositories are also available for event-sourced aggregates. Check the package documentation for the concrete registration type and explicit interface mapping requirements.

MongoDB's configured database name is not selected through the relational `DatabaseTargetResolver`. Do not assume the three relational isolation modes apply to MongoDB. If database-per-tenant MongoDB is required, implement and test that database/client/collection routing as an explicit application concern rather than silently treating `Database:IsolationMode` as active.

## Provider selection checklist

- [ ] Confirm whether the integration is relational EF, EF Cosmos, native Cosmos SDK, or native MongoDB.
- [ ] Confirm which ownership marker/document interface applies.
- [ ] Confirm the partition key or tenant filter is enforced on every access path.
- [ ] Confirm database/container provisioning is automated separately from runtime routing.
- [ ] Test query, update, delete, bulk, raw/native API, and concurrency behavior.
- [ ] Test tenant scope and connection/client reuse across tenants.
- [ ] Document provider limitations and the exact engine/package versions qualified.
