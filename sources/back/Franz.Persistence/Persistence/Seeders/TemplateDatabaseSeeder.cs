using Franz.Common.Data;
using Franz.Common.MultiTenancy;
using Franz.Common.MultiTenancy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Franz.Persistence;

public static class TemplateDatabaseSeeder
{
    /// <summary>
    /// Applies pending migrations, then seeds each active tenant in an isolated DI scope.
    /// </summary>
    public static async Task RunAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        await using (var migrationScope = serviceProvider.CreateAsyncScope())
        {
            var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }

        var tenantStore = serviceProvider.GetRequiredService<ITenantStore>();
        var tenants = await tenantStore.GetAllAsync(cancellationToken).ConfigureAwait(false);

        foreach (var tenant in tenants.Where(tenant => tenant.Status == TenantStatus.Active))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var tenantScope = serviceProvider.CreateAsyncScope();
            var scopedServices = tenantScope.ServiceProvider;
            var tenantContextAccessor = scopedServices.GetRequiredService<ITenantExecutionContextAccessor>();

            tenantContextAccessor.Initialize(new TenantExecutionContext(
                tenant,
                domainId: null,
                source: TenantResolutionSource.Default,
                resolvedAt: DateTimeOffset.UtcNow));

            var seeders = scopedServices.GetServices<ISeeder>()
                .OrderBy(seeder => seeder.Order);

            foreach (var seeder in seeders)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
