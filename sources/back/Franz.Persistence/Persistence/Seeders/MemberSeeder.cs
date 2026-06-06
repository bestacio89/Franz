using System.Linq;
using Franz.Common.Data;
using Franz.Domain.Entities;
using Franz.Domain.ValueObjects;

namespace Franz.Persistence.Seeders;

public sealed class MemberSeeder : ISeeder
{
  private readonly ApplicationDbContext _db;

  // Dependent data runs next
  public int Order => 2;

  public MemberSeeder(ApplicationDbContext db)
  {
    _db = db;
  }

  public async Task  SeedAsync(CancellationToken cancellation)
  {
    if (_db.Members.Any())
    {
      await  _db.Members.AddRangeAsync(_db.Members);
      await _db.SaveChangesAsync();
    }

      await _db.Members.AddRangeAsync(
        new Member(new FullName("John Doe"), new Email("john.doe@example.com")),
        new Member(new FullName("Jane Smith"), new Email("jane.smith@example.com")),
        new Member(new FullName("Alice Johnson"), new Email("alice.j@example.com")),
        new Member(new FullName("Bob Roberts"), new Email("bob.r@example.com")),
        new Member(new FullName("Charlie Brown"), new Email("charlie.brown@example.com")),
        new Member(new FullName("Diana Prince"), new Email("diana.prince@example.com"))
    );

    await _db.SaveChangesAsync();
  }
}