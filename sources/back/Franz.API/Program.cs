using Franz.API.Extensions;
using Franz.Application;
using Franz.Common.Business.Extensions;
using Franz.Common.EntityFramework.Auditing;
using Franz.Common.Http.Bootstrap.Extensions;
using Franz.Common.Http.EntityFramework.Extensions;
using Franz.Common.Logging.Extensions;
using Franz.Common.Mediator.Extensions;
using Franz.Common.Mediator.Polly;
using Franz.Domain.Entities;
using Franz.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
var configuration = builder.Configuration;

// Logging and telemetry are configured once at the host boundary.
builder.Host.UseLog();
builder.Services.AddFranzTelemetry(environment, configuration);

// Register application services and domain-level Franz business capabilities.
builder.Services.RegisterApplicationServices();
builder.Services.AddBusiness(typeof(Book).Assembly);

// Database provider selection is configuration-driven and remains outside the domain.
builder.Services.AddRelationalDatabase<ApplicationDbContext>(environment, configuration);
builder.Services.AddFranzAuditing();

// Register persistence concerns: repositories, cache integration, and persistence services.
builder.Services.RegisterPersistenceServices<ApplicationDbContext>(configuration);

// HTTP architecture owns the common API stack: controllers, routing, auth, and error handling.
builder.Services.AddHttpArchitecture(environment, configuration);

// Cross-cutting mediator behavior is explicit and composable.
builder.Services
    .AddFranzSerilogAuditPipeline()
    .AddFranzSerilogLoggingPipeline();

builder.Services.AddFranzResilience(configuration);

var app = builder.Build();

// Runtime schema changes and development seed data are deliberately limited to Development.
// Production migrations should be applied by the deployment/orchestration pipeline.
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await TemplateDatabaseSeeder.RunAsync(scope.ServiceProvider, app.Lifetime.ApplicationStopping);
}
else
{
    Log.Information(
        "Runtime database migrations are disabled outside Development; apply migrations through deployment.");
}

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.UseHttpArchitecture();

if (app.Environment.IsDevelopment())
{
    app.UseDocumentation();
}

app.MapControllers();

app.Run();

public partial class Program { }
