using Franz.API.Extensions;
using Franz.Application;
using Franz.Common.Http.Bootstrap.Extensions;
using Franz.Common.Logging.Extensions;
using Franz.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = builder.Configuration;

// =========================================================================
// 1. HOST & OBSERVABILITY SUBSYSTEM
// =========================================================================
builder.Host.UseLog();
builder.Services.AddFranzTelemetry(env, config);

// =========================================================================
// 2. SELF-HEALING ARCHITECTURE CORE (AUTONOMOUS DISCOVERY)
// =========================================================================
builder.Services.RegisterApplicationServices();

// Manages Caching, DbContext, and Automated Generic CRUD Topologies
builder.Services.RegisterPersistenceServices<ApplicationDbContext>(config);

// 🔥 AUTOMATIC DISCOVERY GATE: Hydrates Web Stack, Controllers, Routing, 
// Auth, Swagger, Versioning, Mediator Handlers, and ISeeder Implementations.
builder.Services.AddHttpArchitecture(env, config);

var app = builder.Build();

// =========================================================================
// 3. LIFECYCLE & ENVIRONMENT BOUNDARY STATES
// =========================================================================
using (var scope = app.Services.CreateScope())
{
  if (app.Environment.IsDevelopment())
  {
    // One engine call evaluates, migrates, and runs seeders in explicit contract order
    TemplateDatabaseSeeder.Run(scope.ServiceProvider);
  }
  else
  {
    Log.Information("Higher Environment detected. Runtime database migrations blocked.");
  }
}

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

// =========================================================================
// 4. MIDDLEWARE PIPELINE
// =========================================================================
app.UseHttpArchitecture();

if (app.Environment.IsDevelopment())
{
  app.UseDocumentation();
}

var _ = app.MapControllers();

app.Run();