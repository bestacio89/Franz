# Multi-tenancy integration

This page is retained as a stable entry point. The complete Franz.Common 3.0.0 guidance is now organized by concern:

- [Multi-tenancy overview](multitenancy/README.md)
- [Fundamentals and trust boundaries](multitenancy/fundamentals.md)
- [Database isolation modes](multitenancy/database-isolation.md)
- [HTTP integration](multitenancy/http.md)
- [Messaging integration](multitenancy/messaging.md)
- [gRPC integration](multitenancy/grpc.md)
- [Tenant-aware caching](multitenancy/caching.md)
- [Background jobs](multitenancy/background-jobs.md)
- [Provisioning and migrations](multitenancy/provisioning-migrations.md)
- [Testing strategy](multitenancy/testing.md)
- [Persistence provider guidance](multitenancy/provider-guidance.md)

The template defaults to tenant-required HTTP and messaging boundaries with `SharedDatabase` relational persistence. Each service must provide its authoritative tenant store and boundary-specific resolution/authorization policies. Do not disable fail-closed settings merely to make startup succeed.
