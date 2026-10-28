# Service template setup

This template targets .NET 10 and uses the Franz.Common 2.3.3 package baseline.

## Configuration

Configuration is supplied by `appsettings.json`, environment-specific configuration, environment variables, and deployment secret stores. Never commit credentials.

The API defaults to PostgreSQL for local development. The relational provider is selected by `Database:Provider` and currently supports the providers exposed by Franz.Common 2.3.3: `postgres`, `mariadb`, `sqlserver`, and `oracle`. The domain and persistence projects do not reference a provider-specific Franz package.

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
