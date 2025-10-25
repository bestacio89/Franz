// Queries/Books/GetBookByTitleQuery.cs
using Franz.Contracts.DTOs;
using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;

namespace Franz.Contracts.Queries.Books
{
    public sealed record GetBookByTitleQuery(string Title) : IQuery<Result<IEnumerable<BookDto>>>;
}
