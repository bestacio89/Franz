using Franz.API.Extensions;
using Franz.Application;
using Franz.Application.Books.Queries;
using Franz.Common.EntityFramework;
using Franz.Common.Http.Bootstrap.Extensions;
using Franz.Common.Http.Client.Extensions;
using Franz.Common.Http.EntityFramework.Extensions;
using Franz.Common.Logging.Extensions;
using Franz.Common.Mediator.Extensions;
using Franz.Common.Mediator.Polly;
using Franz.Common.Serialization.Extensions;
using Franz.Persistence;
using Franz.Persistence.Seeders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = builder.Configuration;

// =========================
// LOGGING (CLEANED)
// =========================
builder.Host.UseLog();
builder.Services.AddFranzSerilogAuditPipeline()
                .AddFranzEventValidationPipeline()
                .AddFranzSerilogLoggingPipeline()
                .AddFranzTelemetry(env, config);

// =========================
// CORE WEB LAYER
// =========================
builder.Services.AddControllers();

// ⚠️ Choose ONE OpenAPI pipeline
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// APPLICATION LAYER
// =========================
builder.Services.RegisterApplicationServices();

// =========================
// PERSISTENCE
// =========================
builder.Services.RegisterPersistenceServices<ApplicationDbContext>(config);

builder.Services
    .AddRelationalDatabase<ApplicationDbContext>(env, config)
    .AddEntityRepositories<ApplicationDbContext>();

// =========================
// HTTP ARCHITECTURE (FRANZ)
// =========================
builder.Services.AddHttpArchitecture(env, config);

// =========================
// MEDIATOR
// =========================
builder.Services.AddFranzMediator(new[]
{
    typeof(ListBooksQueryHandler).Assembly
});

// =========================
// RESILIENCE
// =========================
builder.Services.AddFranzResilience(config);

// =========================
// API VERSIONING / CORS
// =========================
builder.Services.AddApiVersioning(options =>
{
  options.DefaultApiVersion = new ApiVersion(1, 0);
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.ReportApiVersions = true;
});

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", p =>
      p.AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader());
});

var app = builder.Build();

// =========================
// DB INITIALIZATION (SAFE)
// =========================
using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

  if (env.IsDevelopment())
  {
    // SAFE DEV STRATEGY: no destructive rebuilds
    db.Database.Migrate();
    BookSeeder.Seed(db);
    MemberSeeder.Seed(db);
  }
  else
  {
    db.Database.Migrate();
  }
}

// ensure Serilog flush
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

// =========================
// MIDDLEWARE
// =========================
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// =========================
// OPENAPI (SINGLE PIPELINE)
// =========================
if (env.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Franz API v1");
    c.RoutePrefix = "swagger";
  });
}

app.Run();