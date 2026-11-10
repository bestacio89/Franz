# Tenant-aware caching

Cache isolation is part of the tenant security boundary. A correct database filter does not prevent a cache collision if two tenants can address the same cache key.

## Tenant-scoped cache keys

Franz.Common.Caching provides an opt-in tenant-aware key strategy:

```csharp
services.AddFranzMemoryCaching();
services.AddTenantScopedCacheKeys();
```

For Redis or another provider, register the cache provider first and then register `AddTenantScopedCacheKeys()`. The extension replaces the global cache-key strategy with a scoped strategy that prefixes keys with the validated tenant ID.

When enabled:

- A validated tenant execution context is mandatory.
- Missing context throws; it does not fall back to a global key namespace.
- The current tenant is part of the cache namespace.
- Tenant-sensitive cache operations must execute within the correct request/message/job scope.

The template registers tenant-scoped cache keys through its persistence/cache composition. Keep that registration after the cache provider registration.

## Global and tenant-owned cache entries

Not every cache entry is tenant-owned. Separate global reference data from tenant-owned data explicitly. A global key is safe only when the cached value is genuinely identical and visible across all tenants.

Do not create a tenant-specific key by concatenating an untrusted tenant ID from a request. Use the validated execution context. Include other dimensions that affect the result, such as locale, permissions, feature configuration, or data version, where applicable.

## Invalidation

Cache invalidation must follow the same ownership model as cache population. When tenant data changes, invalidate entries within that tenant's namespace. When global data changes, invalidate the global entry. Avoid broad cross-tenant invalidation as a normal application path.

For multi-instance deployments, account for provider consistency and invalidation propagation. A local in-memory cache is not a distributed coherence mechanism.

## Tenant registry cache is different

`ITenantStore` caching is a separate optional decorator, not the same feature as tenant-scoped application cache keys. It caches tenant metadata lookups in process-local memory. After tenant status, domain, or registry changes, invalidate the affected metadata on every service instance or explicitly accept the configured TTL as the maximum stale-read interval.

Revocation-sensitive operations must not rely on a stale tenant registry cache. Choose the invalidation strategy according to the maximum acceptable revocation delay.

## Mediator query caching

Mediator query caching must include tenant scope when query results are tenant-dependent. Register the tenant-aware key strategy before resolving the key strategy/provider. Commands must not be cached as queries merely because they implement overlapping interfaces; follow the framework's query-caching contract and register source-generated JSON metadata where required for Native AOT.
