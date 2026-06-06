using System;
using System.Linq;
using Franz.Common.Data;
using Franz.Domain.Entities;
using Franz.Domain.ValueObjects;

namespace Franz.Persistence.Seeders;

public sealed class BookSeeder : ISeeder
{
  private readonly ApplicationDbContext _db;

  // Ordered first because other domains might have foreign key relations to Books
  public int Order => 1;

  public BookSeeder(ApplicationDbContext db)
  {
    _db = db;
  }

  public async Task SeedAsync(CancellationToken cancellation)
  {
    if (_db.Books.Any())
    {
      await _db.Books.AddRangeAsync(_db.Books);
      await _db.SaveChangesAsync();
    }

    await _db.Books.AddRangeAsync(
        new Book(new ISBN("978-0321125217"), new Title("Domain-Driven Design"), new Author("Eric Evans"), new DateTime(2003, 8, 30, 0, 0, 0, DateTimeKind.Utc), 5) { Id = 1 },
        new Book(new ISBN("978-0132350884"), new Title("Clean Code"), new Author("Robert C. Martin"), new DateTime(2008, 8, 1, 0, 0, 0, DateTimeKind.Utc), 7) { Id = 2 },
        new Book(new ISBN("978-0134494166"), new Title("Clean Architecture"), new Author("Robert C. Martin"), new DateTime(2017, 9, 20, 0, 0, 0, DateTimeKind.Utc), 4) { Id = 3 },
        new Book(new ISBN("978-0137081073"), new Title("The Clean Coder"), new Author("Robert C. Martin"), new DateTime(2011, 5, 23, 0, 0, 0, DateTimeKind.Utc), 6) { Id = 4 },
        new Book(new ISBN("978-0984782802"), new Title("Pragmatic Thinking for Software Engineers"), new Author("Robert J. Price"), new DateTime(2012, 3, 15, 0, 0, 0, DateTimeKind.Utc)) { Id = 5 },
        new Book(new ISBN("978-0984782819"), new Title("Advanced Patterns in C#"), new Author("Robert J. Price"), new DateTime(2014, 7, 10, 0, 0, 0, DateTimeKind.Utc), 2) { Id = 6 },
        new Book(new ISBN("978-0984782826"), new Title("Microservices in Practice"), new Author("Robert J. Price"), new DateTime(2016, 11, 5, 0, 0, 0, DateTimeKind.Utc), 5) { Id = 7 }
    );

    await _db.SaveChangesAsync();
  }
}