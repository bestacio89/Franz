using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Franz.Common.Data;

namespace Franz.Persistence;

public static class TemplateDatabaseSeeder
{
  /// <summary>
  /// Executes all self-discovered ISeeder implementations sequentially based on their defined priority.
  /// </summary>
  public static void Run(IServiceProvider serviceProvider)
  {
    var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

    // Relational schema migration safe-check
    db.Database.Migrate();

    // Self-discovered array resolution
    var seeders = serviceProvider.GetServices<ISeeder>();

    foreach (var seeder in seeders.OrderBy(s => s.Order))
    {
      seeder.SeedAsync();
    }
  }
}