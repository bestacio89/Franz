# Service template setup

This template targets .NET 10 and uses the Franz.Common 2.3.4 package baseline.

## Configuration

Configuration is supplied by `appsettings.json`, environment-specific configuration, environment variables, and deployment secret stores. Never commit credentials.

The API defaults to PostgreSQL for local development. The relational provider is selected by `Database:Provider` and currently supports the providers exposed by Franz.Common 2.3.4: `postgres`, `mariadb`, `sqlserver`, and `oracle`. The domain and persistence projects do not reference a provider-specific Franz package.

Override database values through environment variables, for example:

- `Database__Provider=Postgres`
- `Database__ServerName=localhost`
- `Database__DatabaseName=service_dev`
- `Database__UserName=postgres`
- `Database__Password=<secret-from-your-secret-store>`
- `Database__Port=5432`

The Redis cache connection is configured through `ConnectionStrings:Redis` (`ConnectionStrings__Redis` in environment variables). The local default is `localhost:6379,abortConnect=false`; provide a managed Redis endpoint and credentials through deployment configuration in non-local environments.

The consumer host's Kafka options are bound under `Messaging:Kafka`. Required settings include `BootstrapServers` and `GroupId`. Kafka security credentials belong in environment variables or a secret store, not in the repository. Outbox settings are under `Messaging:Outbox`.

## Runtime lifecycle

- API and consumer hosts compose Franz.Common services at the host boundary.
- The API runs migrations and ordered seeders only in the Development environment.
- Production schema migrations belong to the deployment/orchestration pipeline.
- The consumer registers the Kafka hosted listener and outbox listener with the .NET Generic Host so their lifecycle is coordinated with application shutdown.
- The relational provider is selected through configuration rather than by a provider-specific reference in the persistence project.

## Validation

The GitHub Actions workflow runs on pull requests targeting `develop` and can also be invoked manually. It restores, builds in Release, and runs the solution tests using the .NET 10 SDK.

## HTTP multi-tenancy

The API owns tenant resolution composition and uses the Franz.Common multi-tenancy contracts and middleware. The HTTP pipeline resolves a tenant after authentication and before authorization.

All three resolution mechanisms are independently configurable under `MultiTenancy`:

- `Resolution:Header:Enabled` with `Header:Name` (default `X-Tenant-Id`).
- `Resolution:Host:Enabled` using verified host mappings from the tenant registry.
- `Resolution:JwtClaim:Enabled` with `JwtClaim:Name` (default `tenant_id`).

When more than one mechanism supplies an identity, all successful identities must agree. Invalid supplied identifiers and contradictory identities fail resolution. A tenant identifier is only a candidate: authenticated requests must also carry the configured membership claim (default `tenant_ids`) containing the selected tenant GUID. Header or host values never grant membership.

The template uses a configuration-backed `ITenantStore` as an executable example. Configure tenants under `MultiTenancy:Tenants`, with `Id`, `Name`, optional `Status`, and a `Domains` array. Host mappings are treated as verified only when explicitly configured in this trusted server-side registry. Replace this store with a database-backed implementation for dynamically managed tenant registries; do not populate it from request data.

Multi-tenancy is required by default. An empty tenant registry therefore rejects requests until a tenant is configured. Keep registry data and membership claims under trusted identity/configuration management.
