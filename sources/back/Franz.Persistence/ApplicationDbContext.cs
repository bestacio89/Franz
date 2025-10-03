using Microsoft.EntityFrameworkCore;
using Franz.Common.Business.Domain;
using Franz.Common.EntityFramework;
using MediatR;
using Franz.Common.Mediator.Dispatchers;

namespace Franz.Persistence
{
  public class ApplicationDbContext : DbContextBase
    
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions, IDispatcher mediator)
        : base(dbContextOptions, mediator)
    {
    }

    // Define DbSet properties for your entity types if needed.
    // Example:
    // public DbSet<MyEntity> MyEntities { get; set; }

    // Additional configuration specific to ApplicationDbContext
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Additional configuration specific to ApplicationDbContext
    }
  }
}
