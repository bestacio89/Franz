# Background jobs and scheduled work

Background work has no implicit HTTP caller. A job that accesses tenant-owned data must establish tenant identity through an explicit, authenticated execution boundary.

## Required job contract

A tenant-scoped job should carry a stable tenant identifier and the minimum additional metadata required to authorize the job. That metadata must be created by a trusted producer, protected against tampering, and validated when the job executes.

At execution time:

1. Resolve the tenant from the authoritative tenant store.
2. Reject unknown, inactive, suspended, or otherwise ineligible tenants.
3. Validate the job's producer/service identity and its permission to execute for that tenant.
4. Resolve and verify any domain context if the operation is domain-scoped.
5. Establish the validated tenant execution context for one job execution scope.
6. Resolve tenant-sensitive dependencies inside that scope.
7. Dispose the scope and clear the context when the job finishes, fails, or is cancelled.

The job payload's tenant ID is a selector, not authorization proof. A job must not be able to change its tenant by editing a payload field.

## Scope isolation

Create a fresh dependency-injection scope for each job execution. Never:

- Capture an HTTP request accessor and use it after the request ends.
- Reuse a scoped accessor or DbContext across jobs.
- Store tenant identity in static or singleton mutable state.
- Execute multiple tenants concurrently through the same scoped context.
- Keep tenant-specific objects in a process-wide cache without tenant-aware keys.

If a job fans out across tenants, create a separate scope for every tenant execution.

## Integration with Franz.Common

The HTTP and messaging adapters establish execution contexts for their own boundaries. The messaging integration already owns the per-delivery lifecycle for supported hosted consumers. For a scheduler or custom job runner, implement an explicit job-boundary adapter that performs the same resolution, validation, and authorization sequence before initializing the scoped `ITenantExecutionContextAccessor`.

Do not call the accessor's initialization method with an unvalidated payload value. Do not invent a shortcut that bypasses the tenant store or authorization policy. Keep this adapter close to the scheduler integration and cover it with cross-tenant tests.

## TenantDatabase and DomainDatabase jobs

A job using `TenantDatabase` must establish the validated tenant context before the DbContext is first resolved. A job using `DomainDatabase` must also establish a verified domain context. Missing identity must fail; the job runner must not silently route to a shared database.

## Retries and delayed execution

Revalidate tenant status and job authorization on every attempt. A job that was authorized when enqueued may become unauthorized before execution. Preserve idempotency keys and correlation identifiers, but do not treat them as tenant authorization.

For recurring jobs, resolve tenant membership at each scheduled execution rather than capturing a tenant context when the schedule is registered.
