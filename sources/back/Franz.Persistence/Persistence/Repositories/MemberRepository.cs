using Franz.Common.DependencyInjection;
using Franz.Contracts.Persistence;
using Franz.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Franz.Persistence.Persistence.Repositories;
internal class MemberRepository: IMemberRepository, IScopedDependency
{
  private readonly ApplicationDbContext _dbContext;

  public MemberRepository(ApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<IReadOnlyList<Member>> GetByPredicateAsync(
       Expression<Func<Member, bool>> predicate,
       CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<Member>()
                          .AsNoTracking()
                          .Where(predicate)
                          .ToListAsync(cancellationToken);
  }

}
