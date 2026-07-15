```markdown
<p align="center">
  <img width="200" src="./Docs/assets/FranzTemplate.png" alt="Franz API Template Logo"/>
</p>

<h1 align="center">Franz API Template</h1>

<p align="center">
  <b>Enterprise .NET Service Industrialization Platform</b>
</p>

<p align="center">
  <b>Architecture as Code • DDD • CQRS • Event Driven • Cloud Ready</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10%2B-blueviolet" />
  <img src="https://img.shields.io/badge/Architecture-Clean%20%7C%20DDD%20%7C%20CQRS-brightgreen" />
  <img src="https://img.shields.io/badge/Messaging-Kafka%20%7C%20RabbitMQ-orange" />
  <img src="https://img.shields.io/badge/Observability-OpenTelemetry-yellow" />
  <img src="https://img.shields.io/badge/Cloud-Azure%20%7C%20AWS%20%7C%20GCP-blue" />
  <img src="https://img.shields.io/badge/IaC-Terraform%20%7C%20Bicep-success" />
</p>

---

# ⚡ Overview — Architecture as Code

Franz API Template is a proprietary enterprise application template designed to
accelerate the creation of production-grade .NET services.

It provides a complete architectural foundation built around:

- Domain Driven Design
- Clean Architecture
- CQRS
- Event Driven Architecture
- Cloud-native deployment patterns
- Observability
- Resilience engineering
- Automated architecture governance

The objective is simple:

> Reduce months of architectural groundwork into a deterministic,
> repeatable engineering foundation.

Franz API Template is not a code generator.

It is an opinionated engineering platform where architecture decisions,
operational practices, and production patterns are encoded from day one.

---

# 🏛 Relationship With Franz.Common

Franz API Template is built on top of the open Franz.Common ecosystem.

Franz.Common provides reusable infrastructure capabilities:

- Dependency injection extensions
- Mediator pipelines
- Messaging abstractions
- Persistence patterns
- Observability integrations
- Cross-cutting application foundations

The API Template adds the enterprise application structure:

- Service boundaries
- Domain organization
- Infrastructure layout
- CI/CD foundations
- Infrastructure as Code
- Architecture enforcement
- Production deployment patterns

Architecture is the product.

The template exists to preserve architectural consistency across projects.

---

# ⚠️ Independence Notice

This project is an independent software project created by:

**Bernardo Estacio Abreu**

It is not affiliated with, sponsored by, endorsed by, or connected to any
company, organization, or product using the name "Franz".

The name Franz in this project originates from the Kafka ecosystem reference
and represents this software ecosystem only.

---

# ✨ Core Capabilities

## 🏗 Architecture Enforcement

The template is designed around the principle:

> Architecture should be executable, not documented.

Included patterns:

- Layer isolation
- Domain boundaries
- Dependency direction enforcement
- CQRS separation
- Repository conventions
- Handler conventions
- DTO contracts
- Infrastructure isolation

Architectural violations should fail before reaching production.

---

# 🧩 Application Structure

Generated services follow a deterministic structure:

```

ServiceName
│
├── ServiceName.Api
│   ├── Controllers
│   ├── Middleware
│   └── Configuration
│
├── ServiceName.Application
│   ├── Commands
│   ├── Queries
│   ├── Handlers
│   ├── Validators
│   └── Mappings
│
├── ServiceName.Domain
│   ├── Entities
│   ├── Aggregates
│   ├── ValueObjects
│   ├── DomainEvents
│   └── Rules
│
├── ServiceName.Infrastructure
│   ├── Persistence
│   ├── Messaging
│   ├── ExternalServices
│   └── Configuration
│
├── ServiceName.Tests
│
└── Infrastructure
├── Terraform
├── Bicep
└── Docker

```

The structure is designed to remain maintainable as the service grows.

---

# 🚀 Creating a New Service

The template is designed to be cloned into a new service.

Example:

```

Franz API Template
|
|
+---- CustomerService
|
+---- OrderService
|
+---- PaymentService

```

Each generated service becomes an independent application.

The generated application belongs to the organization using the template.

The template itself remains proprietary.

---

````markdown
# 🧠 Engineering Philosophy

Franz API Template follows a strict engineering philosophy:

> Defaults are architecture decisions.

The goal is not to provide a blank project.

The goal is to provide a production-ready starting point where the difficult
decisions have already been made.

The template promotes:

- Explicit boundaries
- Strong domain ownership
- Deterministic dependency flow
- Operational readiness
- Repeatable deployment

---

# 📦 Technology Foundation

## Runtime

- .NET 10+
- ASP.NET Core
- Entity Framework Core
- Docker-first execution

---

## Application Architecture

Built around:

- Domain Driven Design
- Clean Architecture
- CQRS
- Event-driven workflows
- Dependency inversion

The application layer coordinates behavior.

The domain owns business rules.

Infrastructure provides technical capabilities.

---

# 🔄 Mediator Pipeline

Franz-based applications use a pipeline-oriented execution model.

Example:

```csharp
builder.Services
    .AddFranzMediatorDefault()
    .AddFranzEventValidationPipeline()
    .AddMediatorOpenTelemetry()
    .AddMediatorEventOpenTelemetry();
````

The pipeline provides:

* Validation
* Logging
* Correlation propagation
* Telemetry
* Resilience policies
* Cross-cutting execution rules

---

# 📡 Messaging Architecture

The template supports event-driven communication patterns.

Supported messaging providers:

* Kafka
* RabbitMQ
* Azure Event Grid
* Azure Service Bus

Typical workflow:

```
Domain Event
      |
      v
Outbox
      |
      v
Message Broker
      |
      v
Consumer
      |
      v
Application Handler
```

The architecture supports asynchronous communication while maintaining domain
boundaries.

---

# 🔒 Resilience Engineering

Production services require predictable failure behavior.

Integrated resilience patterns include:

* Retries
* Timeouts
* Circuit breakers
* Bulkheads
* Fallback strategies

Example:

```csharp
builder.Services.AddFranzResilience(
    builder.Configuration);
```

Failure handling is treated as part of the architecture, not an afterthought.

---

# 📊 Observability

Every service is prepared for operational monitoring.

Included foundations:

## Logging

* Structured logging
* Serilog integration
* Correlation identifiers

## Distributed tracing

* OpenTelemetry instrumentation
* Activity propagation
* Service dependency visibility

## Monitoring

Designed for:

* Elastic Stack
* Azure Monitor
* OpenTelemetry collectors
* Cloud-native observability platforms

---

# 🐳 Container First

Services include container-ready foundations.

Characteristics:

* Multi-stage Docker builds
* Minimal runtime images
* Non-root execution
* Health endpoints
* Environment-based configuration

Example:

```bash
docker build -t service-name .

docker run \
  -p 8080:8080 \
  service-name
```

---

# ☁ Multi-Cloud Infrastructure

Franz API Template is designed for cloud portability.

Supported infrastructure patterns:

## Azure

Using:

* Bicep
* Azure Container Apps
* AKS
* Key Vault
* Managed identities

---

## AWS

Using:

* Terraform
* ECS
* EKS
* RDS
* Networking modules

---

## Google Cloud

Using:

* Terraform
* GKE
* Cloud Run
* Pub/Sub patterns

---

# 🔧 Infrastructure as Code

Infrastructure is treated as versioned software.

Example:

```
Infrastructure
│
├── terraform
│   ├── aws
│   └── gcp
│
├── bicep
│   └── azure
│
└── docker
```

Benefits:

* Repeatable environments
* Reviewable infrastructure changes
* Automated provisioning

---

# 🔄 CI/CD Ready

The template includes foundations for multiple CI/CD ecosystems.

Supported platforms:

| Platform       | Purpose                         |
| -------------- | ------------------------------- |
| GitHub Actions | GitHub-native automation        |
| GitLab CI      | Enterprise pipelines            |
| Azure DevOps   | Microsoft ecosystem deployments |

Typical pipeline stages:

```
Restore
   |
Build
   |
Unit Tests
   |
Architecture Tests
   |
Security Checks
   |
Container Build
   |
Deployment
```

---

# 🧪 Architecture Validation

Architecture rules should be executable.

Typical validation rules:

* Handlers follow naming conventions
* DTOs remain isolated
* Dependencies respect boundaries
* Domain does not depend on infrastructure
* Repository contracts remain consistent

Example principle:

```
Domain
  |
  v
Application
  |
  v
Infrastructure

Never:

Infrastructure
       |
       v
Domain
```

---

# 🛠 Developer Experience

The template aims to provide an identical engineering environment across teams.

Recommended tooling:

* Visual Studio / Rider
* Docker
* Git
* Terraform tooling
* Bicep tooling
* YAML tooling
* Markdown Mermaid preview

The objective:

> Same architecture, same workflow, every developer.

---

```markdown id="qk9sm2"
# 🏗 Service Creation Workflow

Franz API Template is designed to eliminate repetitive project initialization.

A new service is created by cloning the template foundation.

Example:

```

Franz API Template

```
    |
    |
    +---- Customer Service
    |
    +---- Inventory Service
    |
    +---- Payment Service
    |
    +---- Notification Service
```

```

Each generated service becomes an independent application with:

- Its own repository
- Its own deployment lifecycle
- Its own configuration
- Its own domain model

The template provides the foundation.

The generated application becomes the customer's software product.

---

# 🔄 Template Industrialization Workflow

Typical enterprise workflow:

```

1. Clone Franz API Template

   ```
        |
        v
   ```

2. Rename service boundaries

   ```
        |
        v
   ```

3. Connect new repository

   ```
        |
        v
   ```

4. Configure infrastructure

   ```
        |
        v
   ```

5. Implement business domain

   ```
        |
        v
   ```

6. Deploy using existing pipelines

````

The purpose is not generating code.

The purpose is preserving engineering standards while allowing teams to focus on business capabilities.

---

# 📁 Repository Initialization

After creating a service from the template:

```bash
git init

git remote add origin <repository-url>

git add .

git commit -m "Initialize service from Franz API Template"

git push -u origin main
````

The generated repository should become the ownership boundary of the organization using it.

---

# 🧱 Enterprise Usage Model

Franz API Template is intended for:

* Software companies
* Enterprise development teams
* Product teams
* Consulting organizations
* Internal platform teams

Typical use cases:

* Creating microservice ecosystems
* Standardizing backend development
* Reducing architectural drift
* Accelerating delivery of new products

---

# 🎯 Why Use an Application Template?

Most organizations repeatedly rebuild:

* Project structure
* Dependency configuration
* Logging setup
* Messaging infrastructure
* CI/CD pipelines
* Docker configuration
* Cloud infrastructure
* Architecture rules

This creates inconsistency.

Franz API Template turns those repeated decisions into a reusable engineering asset.

---

# 📜 License

Copyright (c) 2025 Bernardo Estacio Abreu

All rights reserved.

This repository contains proprietary software.

The Franz API Template is licensed under the terms described in the
`LICENSE` file.

The license grants authorized users the right to use the template internally
for software development and deployment.

The following actions are prohibited without explicit written permission:

* Redistribution of the template
* Reselling the template
* Sublicensing the template
* Publishing modified versions of the template
* Creating competing template products from this software
* Reverse engineering the template

The ownership of the template remains exclusively with:

**Bernardo Estacio Abreu**

---

# 📌 Generated Application Ownership

Applications created using Franz API Template are separate software products.

Unless otherwise agreed through a commercial agreement:

* The generated application's source code belongs to the organization creating it.
* Business logic implemented by the organization remains its property.
* Domain models created by the organization remain its property.

The license restrictions apply to the template itself, not to independently developed applications created from it.

---

# 🤝 Commercial Licensing

For:

* Enterprise agreements
* Consulting engagements
* Custom template extensions
* Architecture reviews
* Commercial support

Contact:

```
bernardo.estacio89@gmail.com
```

---

# 🦉 Architectural Creed

```
FFFFFFFFF  RRRRRR    AAAAA   N   N  ZZZZZZZ
F         R    R   A     A  NN  N       ZZ
FFFFFF    RRRRRR   AAAAAAA  N N N     ZZZ
F         R   R    A     A  N  NN    ZZ
F         R    R   A     A  N   N   ZZZZZZZ
```

Architecture is not a document.

Architecture is a system of enforceable decisions.

---

# Final Statement

Franz API Template exists to answer one question:

> How do we build production-grade services repeatedly without rebuilding the same foundations every time?

The answer:

**Encode the architecture once. Reuse it everywhere.**

---
