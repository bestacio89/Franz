# gRPC integration

Franz.Common 3.0.0 provides gRPC call context and ordered client/server behavior pipelines. The core package is hosting-runtime independent; the ASP.NET Core adapter applies the server pipeline at the transport boundary.

## Registration

The core registration shape is:

```csharp
using Franz.Common.Grpc.DependencyInjection;

builder.Services
    .AddFranzGrpcDefaults()
    .AddFranzGrpcServer(builder.Configuration)
    .AddFranzGrpcClient(builder.Configuration);
```

Use the ASP.NET Core integration package when hosting gRPC endpoints, and let the host own endpoint mapping, channels, handlers, TLS/mTLS credentials, and generated typed-client lifetimes.

## Tenant security model

On the server, caller-supplied `x-tenant-id` is a selector only. The server must resolve that selector against the authoritative tenant registry and independently authorize the authenticated principal for the resolved tenant before tenant-sensitive application work begins.

Caller-supplied `x-user-id` is not an identity authority. The ASP.NET Core adapter derives the trusted user identifier from the authenticated server-side principal.

The default gRPC authorization service denies calls. Replace it with an explicit application authorization policy before exposing server endpoints. Tenant resolution defaults are no-op until the application supplies the required behavior.

## Server pipeline

The configured server pipeline runs in this order:

1. Exception mapping.
2. Validation.
3. Tenant resolution.
4. Authorization.
5. Logging.
6. Metrics.

The `Franz:Grpc:RequireTenant` option controls whether server-side tenant resolution, validation, and membership authorization are required. Its default is false; enable it for tenant-scoped services and ensure the application has registered the actual resolver and authorization behavior.

The server integration covers unary, server-streaming, client-streaming, and duplex RPCs. Streaming execution must preserve the tenant scope for the full stream lifetime, including enumeration, cancellation, and disposal.

## Client propagation

The client pipeline includes validated tenant propagation. The tenant context must originate from a trusted current execution context. Do not construct outbound tenant metadata from arbitrary request DTO fields.

The client authorization behavior is opt-in. Register it only when the application has an explicit outbound authorization policy. Configure generated typed clients with the standard `Grpc.Net.ClientFactory` registration; Franz's wrappers cover unary calls, while outgoing streaming calls use generated client streaming APIs directly.

## Configuration

```json
{
  "Franz": {
    "Grpc": {
      "RequireTenant": true
    },
    "Client": {
        "DefaultClientTimeout": "00:00:15"
      }
  }
}
```

The documented option paths are `Franz:Grpc:RequireTenant` and `Franz:Grpc:Client:DefaultClientTimeout`. Confirm the exact configuration binding expected by the gRPC registration in the service's referenced package version.

## Operational requirements

- Authenticate the caller using the configured gRPC transport/security model.
- Resolve tenant selectors through authoritative application data.
- Authorize tenant membership independently.
- Apply equivalent checks to streaming RPCs.
- Keep tenant IDs out of metric dimensions to avoid high-cardinality and sensitive telemetry.
- Do not expose raw exception messages or stack traces in remote status details.
- Test missing, inactive, unauthorized, and cross-tenant contexts.
