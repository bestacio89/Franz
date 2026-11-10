# HTTP integration

The HTTP integration in `Franz.Common.Http.MultiTenancy` resolves tenant identity and establishes a validated request-scoped execution context.

## Registration

The template's API composition uses:

```csharp
builder.Services.AddFranzMultiTenancy(options =>
    configuration.GetSection("MultiTenancy:Http").Bind(options));

builder.Services.AddTenantClaimsAuthorizer(options =>
    configuration.GetSection("MultiTenancy:Claims").Bind(options));
```

`AddFranzMultiTenancy` registers the HTTP context accessors, tenant execution accessor, default tenant/domain resolution pipelines, middleware, options validation, and startup validation.

The application must additionally register:

- An `ITenantStore` backed by its authoritative tenant registry.
- One or more `ITenantResolver` strategies.
- An `ITenantRequestAuthorizer` for authenticated tenant membership, unless using the provided GUID-claim authorizer.
- An optional `ITenantValidator` for application-specific eligibility checks.

When `RequireTenant` is enabled, startup validation requires at least one resolver. Authenticated requests fail closed when authenticated tenant authorization is required and no authorizer is registered.

## Configuration

The template's API configuration is:

```json
{
  "MultiTenancy": {
    "Http": {
      "RequireTenant": true,
      "RequireAuthenticatedAuthorization": true,
      "ExcludedPaths": [
        "/health",
        "/health/ready",
        "/health/live"
      ]
    },
    "Claims": {
      "TenantIdClaimType": "tenant_id"
    }
  }
}
```

- `RequireTenant`: reject requests that cannot resolve a valid active tenant.
- `RequireAuthenticatedAuthorization`: require an application authorizer for authenticated tenant membership.
- `ExcludedPaths`: explicit path prefixes that bypass tenant resolution. They are matched on path-segment boundaries. Excluded endpoints must be secured independently.
- `TenantIdClaimType`: claim name used by the built-in GUID claim authorizer.

The built-in `AddTenantClaimsAuthorizer` accepts an authenticated principal whose configured claim parses to the exact resolved tenant GUID. This is a basic membership policy. Use a custom `ITenantRequestAuthorizer` when authorization depends on roles, subscriptions, delegated access, tenant status, or other domain rules.

## Middleware order

When using `Franz.Common.Http.Bootstrap`, `UseHttpArchitecture()` already inserts tenant resolution after authentication and before endpoint authorization. Do not add `UseFranzMultiTenancy()` a second time.

If composing the HTTP pipeline manually, use the middleware once and ensure authentication executes before tenant membership authorization. Do not let controllers or application handlers establish tenant identity from raw request headers.

## Resolver strategy

Implement `ITenantResolver` for the service's chosen selector (for example, a trusted host name, route value, or explicit tenant header). A resolver selects a tenant; it does not authorize the caller.

A secure resolver should:

1. Normalize and validate the selector.
2. Resolve it through the authoritative `ITenantStore`.
3. Reject ambiguous or unknown mappings.
4. Avoid accepting arbitrary forwarding headers unless the proxy chain is explicitly trusted and normalized.
5. Return a tenant resolution result with its source recorded.

Do not register a header resolver and then treat the header as proof of membership. The request authorizer must independently authorize the authenticated principal against the resolved tenant.

## Domain resolution

The HTTP pipeline can resolve an optional domain context. The resulting domain must be verified as belonging to the resolved tenant before it becomes part of the immutable execution context. This verified domain is required for `DomainDatabase` routing.

## Health and operational endpoints

Excluded health paths bypass tenant resolution by design. Keep them narrow. They must not expose tenant data or administrative operations. Protect operational endpoints through network policy, authentication, authorization, or platform-level controls appropriate to the deployment.

## Request-scoped access

Application services should consume `ITenantExecutionContextAccessor`, `ITenantContextAccessor`, or `IDomainContextAccessor` through dependency injection. They should not parse headers or claims themselves. The middleware clears the context when the request ends, including exceptional exits.
