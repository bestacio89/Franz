using Franz.Domain.Entities;
using Franz.Persistence;
using Franz.Common.Business.Domain;
using Franz.Common.EntityFramework;
using Franz.Common.Mediator.Dispatchers;
using Microsoft.EntityFrameworkCore;

namespace Franz.Persistence
{
  public class ApplicationDbContext : DbContextBase
  {
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDispatcher dispatcher // Franz mediator dispatcher
    ) : base(options, dispatcher)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Configure Value Objects for Book
      modelBuilder.Entity<Book>(builder =>
      {
        builder.OwnsOne(b => b.Isbn, isbn =>
        {
          isbn.Property(p => p.Value)
              .HasColumnName("Isbn")
              .IsRequired();
        });

        builder.OwnsOne(b => b.Title, title =>
        {
          title.Property(p => p.Value)
              .HasColumnName("Title")
              .IsRequired();
        });

        builder.OwnsOne(b => b.Author, author =>
        {
          author.Property(p => p.Value)
              .HasColumnName("Author")
              .IsRequired();
        });
      });

      // Configure Value Objects for Member
      modelBuilder.Entity<Member>(builder =>
      {
        builder.OwnsOne(m => m.Name, name =>
        {
          name.Property(p => p.Value)
              .HasColumnName("Name")
              .IsRequired();
        });

        builder.OwnsOne(m => m.Email, email =>
        {
          email.Property(p => p.Value)
              .HasColumnName("Email")
              .IsRequired();
        });
      });

      // Apply seeders (if any)
      // modelBuilder.ApplyConfiguration(new BookSeeder());
      // modelBuilder.ApplyConfiguration(new MemberSeeder());
    }

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;

    // Later: DbSet<Loan>, DbSet<Reservation>, etc.
  }
}
