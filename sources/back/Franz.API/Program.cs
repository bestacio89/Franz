using Franz.API.Extensions;
using Franz.Application;
using Franz.Application.Books.Queries;
using Franz.Common.Http.Bootstrap.Extensions;
using Franz.Common.Http.Client.Extensions;
using Franz.Common.Http.EntityFramework.Extensions;

using Franz.Common.Logging.Extensions;
using Franz.Common.Mediator.Extensions;

using Franz.Common.Mediator.Polly;

using Franz.Persistence; // our new cowboy helper
using Franz.Persistence.Seeders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Filters;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (env-aware Serilog via UseHybridLog) ---
builder.Host.UseLog();
builder.Services.AddFranzSerilogAuditPipeline()
                .AddFranzEventValidationPipeline()
                .AddFranzSerilogLoggingPipeline()
                .AddMediatorOpenTelemetry()
                .AddMediatorEventOpenTelemetry(new System.Diagnostics.ActivitySource("Franz.Mediator"));



// --- Core services ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- Application & Persistence ---
builder.Services.RegisterApplicationServices();
builder.Services.RegisterPersistenceServices<ApplicationDbContext>(builder.Configuration);
builder.Services.AddRelationalDatabase<ApplicationDbContext>(builder.Environment, builder.Configuration)
  .AddEntityRepositories<ApplicationDbContext>()
  .AddGenericRepositories<ApplicationDbContext>();
  
  


// --- Http Architecture ---
builder.Services.AddHttpArchitecture(builder.Environment, builder.Configuration);

// --- Messaging ---
//builder.Services.AddMessagingInHttpContext(builder.Configuration);

//builder.Services.AddHttpServices(builder.Configuration, TimeSpan.FromSeconds(30));
//builder.Services.AddExternalServices(builder.Configuration);

// --- Mediator + Pipelines ---
builder.Services.AddFranzMediator(new[] { typeof(ListBooksQueryHandler).Assembly });
    ;

// --- Resilience (Polly) ---
builder.Services.AddFranzResilience(builder.Configuration);

// --- API Versioning & CORS ---
builder.Services.AddApiVersioning(options =>
{
  options.DefaultApiVersion = new ApiVersion(1, 0);
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.ReportApiVersions = true;
});
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", policy =>
      policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- DB Initialization ---
using (var scope = app.Services.CreateScope())
{
  var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
  var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

  if (env.IsDevelopment())
  {
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    BookSeeder.Seed(db);
    MemberSeeder.Seed(db);
  }
  else
  {
    db.Database.Migrate();
  }
}

// Ensure Serilog flushes
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

// --- Middleware ---
//app.UseCors("AllowAll");
//app.UseHttpArchitecture();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/openapi/v1.json", "Franz API v1");
    c.RoutePrefix = "swagger";
  });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
