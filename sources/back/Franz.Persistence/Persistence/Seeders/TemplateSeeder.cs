using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Franz.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Franz.Persistence;

public static class TemplateDatabaseSeeder
{
  /// <summary>
  /// Applies pending migrations and runs discovered seeders in their declared order.
  /// </summary>
  public static async Task RunAsync(
      IServiceProvider serviceProvider,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(serviceProvider);

    var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

    var seeders = serviceProvider.GetServices<ISeeder>()
        .OrderBy(seeder => seeder.Order);

    foreach (var seeder in seeders)
    {
      cancellationToken.ThrowIfCancellationRequested();
      await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
    }
  }
}
