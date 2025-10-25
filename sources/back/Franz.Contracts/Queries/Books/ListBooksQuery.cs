// Application/Books/Queries/ListBooksQuery.cs
using Franz.Contracts.DTOs;
using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;

namespace Franz.Contracts.Queries.Books;

public sealed record ListBooksQuery
    : IQuery<Result<IReadOnlyCollection<BookDto>>>;
