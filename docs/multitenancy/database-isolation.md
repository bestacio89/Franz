# Database isolation modes

Franz.Common 3.0.0 exposes three database target-selection modes through `Database:IsolationMode` (`DatabaseIsolationMode`). The relational adapters use the shared `DatabaseTargetResolver`.

These modes choose the database name for a DbContext execution. They do not provision databases, select credentials, run migrations, or replace entity-level tenant authorization.

## Decision matrix

| Mode | Database target | Required execution identity | Typical use |
|---|---|---|---|
| `SharedDatabase` | One configured database name | No identity needed for routing; tenant context is still required for tenant-scoped operations | Shared tables with tenant filters |
| `TenantDatabase` | Configured name with `{dbName}` replaced by tenant GUID | Validated tenant execution context | Database per tenant |
| `DomainDatabase` | Configured name with `{dbName}` replaced by verified domain GUID | Verified domain execution context | Database per domain/customer domain boundary |

## 1. SharedDatabase

Use one concrete database name:

```json
{
  "Database": {
    "IsolationMode": "SharedDatabase",
    "DatabaseName": "application"
  }
}
```

The configured name is used unchanged. A `{dbName}` token is invalid in this mode and fails rather than silently selecting a tenant-specific target.

For a multi-tenant shared relational database:

- Tenant-owned entities implement `ITenantEntity` and expose a mapped `Guid TenantId`.
- Intentionally shared entities implement `IGlobalEntity`.
- Every persisted Franz `IEntity<TKey>` must implement exactly one of those ownership markers when multi-tenancy is enabled for the shared database.
- Tenant query filters isolate reads; tracked writes enforce tenant ownership and stamp new tenant-owned rows from the validated context.
- Add tenant-aware indexes based on actual query patterns, commonly composite indexes beginning with `TenantId`.

Example entity shape:

```csharp
public sealed class Customer : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }

    // Domain properties...
}
```

Do not duplicate the tenant ID from a request DTO into persistence as an authority. The framework's validated execution context is the source of tenant ownership for tenant-scoped writes.

## 2. TenantDatabase

Use the `{dbName}` token in the configured database name:

```json
{
  "Database": {
    "IsolationMode": "TenantDatabase",
    "DatabaseName": "application_{dbName}"
  }
}
```

For a validated tenant ID, the resolver substitutes the token with the tenant GUID in canonical `D` format. For example, `application_{dbName}` becomes `application_00000000-0000-0000-0000-000000000001`.

Requirements:

- A validated tenant execution context must exist before the DbContext is resolved.
- The database name must contain `{dbName}`.
- Missing tenant context throws a tenant-isolation failure; it does not fall back to a domain ID or the shared database.
- The application must provision the tenant database, resolve credentials, apply migrations, and manage lifecycle and connection-pool budgets.

A single shared DbContext type can be used if the provider adapter supports this routing and the context is scoped to one execution. Never reuse a context or connection across tenant boundaries.

## 3. DomainDatabase

Use the same token with the explicit domain isolation mode:

```json
{
  "Database": {
    "IsolationMode": "DomainDatabase",
    "DatabaseName": "application_{dbName}"
  }
}
```

The resolver substitutes `{dbName}` with the verified domain GUID in canonical `D` format.

The domain context must already have been verified and associated with the resolved tenant. This mode does not infer a domain from a tenant, and it does not fall back to the tenant ID when the domain context is absent. Missing domain context fails closed.

Use this mode only when the domain is genuinely the database ownership/routing boundary. If a tenant owns multiple domains but all of its data belongs in one database, `TenantDatabase` is usually the more accurate model.

## Resolver behavior

The shared resolver enforces the following invariants:

- `SharedDatabase` rejects a database name containing `{dbName}`.
- `TenantDatabase` requires the token and a non-empty validated tenant ID.
- `DomainDatabase` requires the token and a non-empty verified domain ID.
- Unsupported enum values fail explicitly.

Do not pre-resolve the name in application code or maintain a second routing implementation. Let the provider adapter use the common resolver.

## DbContext integration

For multi-tenant EF Core applications, derive from `DbContextBase` and forward the execution accessor plus both option objects. This allows the framework to apply the configured multi-tenancy and database-isolation behavior:

```csharp
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDispatcher dispatcher,
    ITenantExecutionContextAccessor tenantContext,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<MultiTenancyOptions> multiTenancyOptions)
    : DbContextBase(
        options,
        dispatcher,
        tenantContextAccessor: tenantContext,
        databaseOptions: databaseOptions,
        multiTenancyOptions: multiTenancyOptions)
{
    public DbSet<Customer> Customers => Set<Customer>();
}
```

Use the constructor overload and named arguments supported by the package version referenced by the service. The current template's `ApplicationDbContext` demonstrates the positional equivalent.

## Selecting a mode

Choose based on the actual isolation and operations model:

- Prefer `SharedDatabase` when tenant filters plus database-level controls satisfy the risk and scale requirements.
- Choose `TenantDatabase` when database-per-tenant isolation, independent backup/restore, residency, or tenant-level operational control justifies the operational overhead.
- Choose `DomainDatabase` only when verified domain ownership is the intended database partitioning key.

Do not select database-per-tenant merely as a substitute for authorization. Each mode still needs authenticated tenant access controls, scoped execution identity, and cross-tenant tests.
