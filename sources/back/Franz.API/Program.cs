using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Franz.API.Helpers;
using Franz.Persistence;
using Franz.Common.Business.Domain;
using Franz.Common.Business.Events;
using Franz.Common.EntityFramework.SQLServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Franz.Application;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

var productName = "Franz"; // Replace with your product name
var applicationAssemblyName = Path.Combine(Directory.GetCurrentDirectory(), @"bin\Debug\net8.0", $"{productName}.Application.dll");
var contractsAssemblyName = Path.Combine(Directory.GetCurrentDirectory(), @"bin\Debug\net8.0", $"{productName}.Contracts.dll");
var apiAssemblyName = Path.Combine(Directory.GetCurrentDirectory(), @"bin\Debug\net8.0", $"{productName}.Api.dll");
// Get all loaded assemblies in the current AppDomain

Assembly applicationAssembly = Assembly.LoadFile(applicationAssemblyName);
Assembly contractsAssembly = Assembly.LoadFile(contractsAssemblyName);
Assembly  apiassembly = Assembly.LoadFile(apiAssemblyName);
// Register ApplicationAssembly and ContractsAssembly in the DI container
builder.Services.AddSingleton(applicationAssembly);
  builder.Services.AddSingleton(contractsAssembly);

  var isApplicationAssemblyLoaded = applicationAssembly != null;
  var isContractsAssemblyLoaded = contractsAssembly != null;

  Console.WriteLine($"Is {applicationAssemblyName} loaded: {isApplicationAssemblyLoaded}");
  Console.WriteLine($"Is {contractsAssemblyName} loaded: {isContractsAssemblyLoaded}");

// Read TId type name from configuration

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddHttpArchitecture(
    builder.Environment,
    builder.Configuration);
// Create the generic types


// Register persistence services
builder.Services.RegisterPersistenceServices<ApplicationDbContext>(builder.Configuration);

// Add rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder =>
{
    builder.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100, // Adjust the limit as per your needs
            Period = "1m" // Adjust the period as per your needs (1 minute in this example)
        }
    };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var app = builder.Build();

app.Initialize(); // Initialize your application

app.UseCors("AllowAll"); // Enable CORS

app.UseHttpArchitecture();

app.Run();
