using Franz.Common.Mediator.Dispatchers;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Franz.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
  public ApplicationDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    var config = new ConfigurationBuilder()
      .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Franz.Api"))
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile("appsettings.Development.json", optional: true)
      .Build();

    var db = config.GetSection("Database");

    var connectionString =
        $"Host={db["ServerName"]};" +
        $"Database={db["DatabaseName"]};" +
        $"Username={db["UserName"]};" +
        $"Password={db["Password"]};" +
        $"Port={db["Port"]};" +
        $"SSL Mode={db["Ssl"]};";

    optionsBuilder.UseNpgsql(
            connectionString);

    // Minimal service provider just for design time
    var services = new ServiceCollection();
    services.AddScoped<IDispatcher, FranzDispatcher>();
    var sp = services.BuildServiceProvider();

    return new ApplicationDbContext(
        optionsBuilder.Options,
        sp.GetRequiredService<IDispatcher>());
  }
}