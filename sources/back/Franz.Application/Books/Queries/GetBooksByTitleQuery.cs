


using Franz.Domain.Entities;

using Franz.Common.Mapping.Abstractions;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.DTOs;
using Franz.Contracts.Persistence;
using Franz.Contracts.Queries.Books;

namespace Franz.Application.Books.Queries;
public sealed class GetBookByTitleQueryHandler
    : IQueryHandler<GetBookByTitleQuery, Result<IEnumerable<BookDto>>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IFranzMapper _mapper;

    public GetBookByTitleQueryHandler(IBookRepository bookRepository, IFranzMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<BookDto>>> Handle(
        GetBookByTitleQuery request,
        CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetBooksByAuthorAsync(request.Title, cancellationToken);

        if (books is null || !books.Any())
            return "No books found with the given title".ToFailure<IEnumerable<BookDto>>();

        return _mapper.Map<IEnumerable<Book>,IEnumerable<BookDto>>(books).ToResult();
    }
}
