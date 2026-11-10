using Franz.Common.EntityFramework;
using Franz.Common.EntityFramework.Configuration;
using Franz.Common.Mediator.Dispatchers;
using Franz.Common.MultiTenancy;
using Franz.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Franz.Persistence;

public sealed class ApplicationDbContext : DbContextBase
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDispatcher dispatcher,
        ITenantExecutionContextAccessor tenantExecutionContextAccessor,
        IOptions<DatabaseOptions> databaseOptions,
        IOptions<MultiTenancyOptions> multiTenancyOptions)
        : base(
            options,
            dispatcher,
            null,
            tenantExecutionContextAccessor,
            databaseOptions,
            multiTenancyOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(builder =>
        {
            builder.OwnsOne(b => b.Isbn, isbn =>
            {
                isbn.Property(p => p.Value).HasColumnName("Isbn").IsRequired();
            });

            builder.OwnsOne(b => b.Title, title =>
            {
                title.Property(p => p.Value).HasColumnName("Title").IsRequired();
            });

            builder.OwnsOne(b => b.Author, author =>
            {
                author.Property(p => p.Value).HasColumnName("Author").IsRequired();
            });
        });

        modelBuilder.Entity<Book>()
            .HasIndex(book => book.TenantId);

        modelBuilder.Entity<Member>(builder =>
        {
            builder.HasIndex(member => member.TenantId);

            builder.OwnsOne(m => m.Name, name =>
            {
                name.Property(p => p.Value).HasColumnName("Name").IsRequired();
            });

            builder.OwnsOne(m => m.Email, email =>
            {
                email.Property(p => p.Value).HasColumnName("Email").IsRequired();
            });
        });
    }

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
}
