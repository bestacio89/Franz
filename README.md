# ⚡ Franz Framework  

> **Architecture as Code** — because discipline in engineering is the law, not a democracy.  
Franz enforces clean architecture at compile-time and runtime through **architecture tests, mediator pipelines, logging, and resilience** — shipping day-1 with Docker, CI/CD, and multi-cloud IaC baked in.  

---

## ✨ Features  

- 🏗 **Architecture as Code**: conventions & rules enforced via [ArchUnitNET](https://github.com/TNG/ArchUnitNET). No spaghetti allowed.  
- 📦 **Mediator Pipelines**: CQRS-style command/query separation with resilience, validation, logging, and tracing pipelines.  
- 🔒 **Resilience with Polly**: retry, circuit breaker, bulkhead, timeout, and advanced breakers — all config-driven.  
- 📊 **Logging & Tracing**: env-aware Serilog, OpenTelemetry pipelines, correlation IDs, ELK-friendly enrichers.  
- 📡 **Messaging**: Kafka consumer/producer + RabbitMQ and Azure Event Grid ready.  
- 🐳 **Container-ready**: multi-stage Dockerfile, non-root runtime, healthchecks baked in.  
- ☁ **Cloud-ready IaC**: Terraform + Bicep modules for **Azure**, **AWS**, **GCP** (GKE, Cloud Run, networking, databases, Kafka).  
- 🔄 **Multi-CI/CD pipelines**: templates for **Azure DevOps**, **GitHub Actions**, and **GitLab CI**.  

---

## 🚀 Getting Started  

### Install NuGet packages  

```bash
dotnet add package Franz.Common.Mediator
dotnet add package Franz.Common.Mediator.Polly
dotnet add package Franz.Common.Logging
dotnet add package Franz.Common.Http
````

### Bootstrap API

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseHybridLog();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.RegisterApplicationServices();
builder.Services.RegisterPersistenceServices<ApplicationDbContext>(builder.Configuration);
builder.Services.AddDatabase<ApplicationDbContext>(builder.Environment, builder.Configuration);

// HTTP & Messaging
builder.Services.AddHttpArchitecture(builder.Environment, builder.Configuration);
builder.Services.AddMessagingInHttpContext(builder.Configuration);
builder.Services.AddHttpServices(builder.Configuration, TimeSpan.FromSeconds(30));
builder.Services.AddExternalServices(builder.Configuration);

// Mediator & Resilience
builder.Services.AddFranzMediatorDefault()
    .AddFranzEventValidationPipeline()
    .AddMediatorOpenTelemetry()
    .AddMediatorEventOpenTelemetry(new ActivitySource("Franz.Mediator"));

builder.Services.AddFranzResilience(builder.Configuration);

// API & CORS
builder.Services.AddApiVersioning(o => { o.DefaultApiVersion = new ApiVersion(1, 0); });
builder.Services.AddCors(p => p.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors("AllowAll");
app.UseHttpArchitecture();
app.MapControllers();
app.Run();
```

---

## 🔄 CI/CD Matrix

Franz ships with **multi-CI/CD pipelines out-of-the-box**:

| CI/CD Provider     | Location             | Notes                                |
| ------------------ | -------------------- | ------------------------------------ |
| **Azure DevOps**   | `pipelines/`         | Build, Infra, Publish YAML templates |
| **GitHub Actions** | `.github/workflows/` | Portable jobs for GH-native runners  |
| **GitLab CI**      | `.gitlab/ci/`        | Ready-to-use `.gitlab-ci.yml` chain  |

💡 Pick your provider, drop in secrets, and you’re live.

---

## ☁ Infrastructure as Code

Franz includes **multi-cloud infrastructure modules**:

* **Terraform-GCP**

  * `modules/` → `cloudrun`, `gke`, `kafka`, `networking`, `database`
  * `pipelines/` → ready-to-use GitHub/Azure/GitLab CI jobs for infra & publish

* **Terraform-Infra** (generic modules)

  * Backend / Outputs / Variables boilerplate
  * Easily extendable for AWS + Azure

* **Azure Bicep**

  * `Infrastructure/main.bicep` with modular imports in `Infrastructure/Modules/`

---

## 🐳 Docker

```bash
docker build -t franz-api .
docker run -p 8080:80 franz-api
```

Includes:

* Multi-stage build (`sdk → publish → runtime`)
* Healthcheck endpoint (`/health`)
* Non-root user runtime

---

## 🧪 Architecture Tests

Architecture rules are enforced via `Franz.Testing`:

* ✅ Command handlers must end with `CommandHandler` and implement `ICommandHandler<,>`.
* ✅ Query handlers must end with `QueryHandler` and implement `IQueryHandler<,>`.
* ✅ DTOs must end with `Dto`.
* ✅ Repositories must implement correct lifetimes (`IScopedDependency`, `ISingletonDependency`).

No PR merges unless architecture tests pass.

---

## 📦 Messaging Example

### Kafka Consumer

```csharp
public class KafkaConsumerService : IHostedService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IMessageHandler _handler;

    public KafkaConsumerService(IOptions<MessagingOptions> opts, IMessageHandler handler)
    {
        _consumer = new ConsumerBuilder<string, string>(
            new ConsumerConfig { BootstrapServers = opts.Value.BootStrapServers, GroupId = opts.Value.GroupID }
        ).Build();
        _handler = handler;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _consumer.Subscribe("my-topic");
        Task.Run(() => { while (!ct.IsCancellationRequested) { var msg = _consumer.Consume(ct); _handler.Process(new Message(msg.Message.Value)); }});
        return Task.CompletedTask;
    }
}
```

---

## 📜 Changelog

See [CHANGELOG.md](./CHANGELOG.md) for version history.
Latest release: **1.6.2 — Unified Resilience Bootstrapper**

---

## 👑 Philosophy

Franz = **the vaccine against spaghetti**.

Most companies: *“We enforce architecture with code reviews and Confluence docs.”*
Franz: *“Who said this was a democracy?”*

With Franz, architecture is **not optional**, it’s **codified and enforced**.

---

## ⚡ Quick Pitch

> *“What Franz, and I, can do in 72 hours — most pro teams battle months to achieve.”*

* Day 1: Templates & Framework
* Day 2: Documentation
* Day 3: MVP models ready — **microservices or monolith, doesn’t matter**

---

```

