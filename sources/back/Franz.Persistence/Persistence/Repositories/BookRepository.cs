using Franz.Common.DependencyInjection;

using Franz.Contracts.Persistence;
using Franz.Domain.Entities;

using System.Linq.Expressions;

namespace Franz.Persistence.Repositories;

public class BookRepository : IBookRepository, IScopedDependency
{
    private readonly ApplicationDbContext _dbContext;

    public BookRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

  public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
      => await _dbContext.Books.ToListAsync(cancellationToken);

  public async Task<IReadOnlyList<Book>> GetBooksByAuthorAsync(
        string author,
        CancellationToken cancellationToken = default)
  {
    return await _dbContext.Books
        .Where(b => b.Author.Value.Contains(author))   // 👈 no .Value
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<Book>> GetBooksByTitleAsync(
      string title,
      CancellationToken cancellationToken = default)
  {

    return await _dbContext.Books
        .Where(b => b.Title.Value.Contains(title))   // 👈 no .Value
        .ToListAsync(cancellationToken);
  }
}
