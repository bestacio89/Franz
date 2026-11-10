# Multi-tenancy fundamentals

## Identity model

Franz.Common separates identity from the boundary that establishes it.

- **Tenant**: an independently governed customer, organization, account, or other isolation principal in the service's domain.
- **Domain**: a verified domain associated with a tenant. It can be used for domain-level routing when the application has a trustworthy domain-resolution process.
- **Tenant execution context**: immutable, validated identity attached to one execution scope (HTTP request or message delivery).
- **Tenant-owned entity**: persisted data belonging to one tenant.
- **Global entity**: persisted data intentionally shared across tenants.

A tenant ID is an identifier, not an authorization credential. A hostname, route segment, HTTP header, JWT claim, message header, or gRPC metadata value can help select a tenant. None proves that the caller or producer is allowed to act for that tenant.

## The execution lifecycle

A safe execution follows this sequence:

1. Extract a tenant selector from a trusted boundary.
2. Resolve the selector through an application-registered resolver and authoritative tenant store.
3. Reject unknown, inactive, malformed, or otherwise ineligible tenants.
4. Apply application tenant validation rules where configured.
5. Authorize the authenticated user or transport producer for the resolved tenant.
6. Resolve and verify any optional domain against the tenant.
7. Establish the immutable tenant execution context for the current scope.
8. Execute application and persistence work.
9. Clear/dispose the execution scope when the request or delivery ends.

The HTTP and messaging integrations implement this lifecycle at their respective boundaries. The framework does not infer trusted identity from arbitrary claims or raw metadata.

## Framework contracts and application responsibilities

| Contract/capability | Responsibility |
|---|---|
| `TenantInfo` | Tenant identity, status, and associated domains |
| `ITenantStore` | Authoritative tenant lookup, existence checks, and enumeration |
| `ITenantResolver` | Boundary-specific resolution strategy |
| `ITenantValidator` | Additional application tenant policy |
| `ITenantRequestAuthorizer` | Authenticated HTTP user membership/authorization |
| `ITenantMessageAuthorizer` | Authorization of the authenticated message producer |
| `ITenantExecutionContextAccessor` | Scoped access to the validated execution context |
| `ITenantContextAccessor` / `IDomainContextAccessor` | Tenant/domain identity accessors |
| `ITenantEntity` / `IGlobalEntity` | Explicit persistence ownership classification |

The application must implement the tenant store and authorization policies. The HTTP resolver strategy is also application-registered. The framework provides the boundary pipelines and execution-context mechanics; it does not know your customer registry, subscription rules, user membership model, or broker identity model.

## Tenant and domain identity are not interchangeable

A tenant represents the ownership/security boundary. A domain is an associated, verified routing context. Domain identity must be validated as belonging to the resolved tenant before it is used.

In particular:

- `TenantDatabase` routes using the validated tenant ID.
- `DomainDatabase` routes using a verified domain ID.
- A missing tenant must never fall back to a domain ID.
- A missing domain must never fall back to a tenant ID.
- A client-provided domain string must not be treated as a verified domain identity.

## Scope and lifetime rules

The execution context is scoped, not process-global. Do not store the current tenant in a static field, singleton, shared mutable object, or long-lived DbContext. Do not reuse an HTTP request scope for another tenant or reuse a message-delivery scope for another message.

A message consumer must establish a fresh dependency-injection scope per delivery. Tenant-sensitive dependencies must be resolved inside that scope. The HTTP middleware clears context after downstream execution; the messaging initializer returns an async-disposable scope whose disposal clears the context.

## Security invariants

- Fail closed when required tenant context is missing.
- Resolve against an authoritative tenant registry.
- Authorize membership independently of tenant selection.
- Verify tenant status and any associated domain.
- Scope data access, cache keys, messages, and jobs to validated identity.
- Treat excluded health/operational endpoints as separate security surfaces.
- Audit tenant-sensitive administrative operations.
- Never log credentials, access tokens, or message payloads containing sensitive data.

## Tenant registry caching

Tenant-store caching is a separate concern from tenant-scoped application cache keys. The tenant-store cache is process-local and can become stale across service instances. Invalidate affected tenant metadata after registry changes and distribute invalidation across instances or accept a bounded staleness window explicitly. A TTL alone is not an immediate revocation mechanism.
