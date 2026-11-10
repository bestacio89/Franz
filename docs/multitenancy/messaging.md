# Messaging integration

The messaging integration establishes validated tenant execution context for broker-delivered messages. The same security model applies to Kafka and RabbitMQ consumers.

## Registration

The template's Consumer host uses:

```csharp
builder.Services.AddMessagingMultitenancyContext(options =>
    configuration.GetSection("Messaging:Tenant").Bind(options));
```

The application must register:

- An authoritative `ITenantStore`.
- An `ITenantMessageAuthorizer` that authorizes the authenticated transport producer for the resolved tenant.
- An optional `ITenantValidator` for additional tenant policy.

The initializer also requires the scoped `ITenantExecutionContextAccessor`, registered by the messaging tenancy integration.

## Configuration

```json
{
  "Messaging": {
    "Tenant": {
      "RequireTenantScope": true
    },
    "Context": {
      "IncludeTenant": true,
      "IncludeCorrelationId": true,
      "IncludeUserIdentity": true
    }
  }
}
```

`RequireTenantScope` defaults to false in the package because the integration can also be used for outbound metadata propagation. Enable it for consumers that process tenant-owned data. When enabled, supported hosted consumers validate the required initializer before they start listening.

`Context.IncludeTenant` controls tenant metadata propagation for outgoing messages. It does not grant the producer permission to publish for that tenant.

## Message contract and trust

A tenant-scoped message must carry exactly one valid, non-empty GUID value in the standard tenant header. An optional domain header must also contain exactly one valid GUID and resolve to a verified domain associated with the tenant.

The initializer:

1. Reads the tenant/domain metadata from the message context.
2. Resolves the tenant through `ITenantStore`.
3. Rejects unknown or inactive tenants.
4. Invokes the optional `ITenantValidator`.
5. Verifies any supplied domain association.
6. Calls `ITenantMessageAuthorizer` to authorize the producer.
7. Establishes the tenant execution context for the message scope.

The message header is untrusted input. The authorizer must use trusted broker/transport identity or another authenticated producer identity. An arbitrary tenant header or payload field is not proof of producer authorization.

## Execution scope

Supported hosted consumer paths create a fresh dependency-injection scope per delivery and invoke the tenant initializer before selecting/executing the handler. The initializer's async-disposable scope is disposed after the handler pipeline on success, failure, or cancellation.

Consequences:

- Resolve tenant-sensitive services inside the per-message scope.
- Do not capture scoped services in singleton handlers or hosted-service constructors.
- Do not reuse a DbContext or tenant accessor between deliveries.
- Do not manually initialize the context again when the hosted adapter already owns the lifecycle.
- Do not allow message processing to outlive the tenant scope.

For custom transport integrations, use `ITenantMessageContextInitializer.BeginAsync(message, cancellationToken)` and keep the returned async-disposable scope alive for the complete handler execution. Clear ambient message context as part of the same lifecycle.

## Outbound propagation

Outgoing message builders can propagate the current validated tenant/domain context. Propagation preserves context; it does not replace downstream tenant lookup and producer authorization.

Every service boundary must independently validate the incoming context. Do not assume a message is trustworthy because another internal service produced it. Use broker ACLs, authenticated producer identities, topic/queue permissions, and application-level authorizers together.

## Dead letters, retries, and outbox

Preserve tenant metadata across retries and dead-letter flows only when the transport contract and security model permit it. Ensure the consumer revalidates tenant status and producer authority on every delivery. An outbox record should retain the tenant context required to publish the event, but a stale outbox tenant ID is not an authorization credential.

The outbox listener must resolve stores, serializers, and dispatch dependencies within the message execution scope. Verify the actual transport adapter's behavior when adding a new provider or custom listener.
