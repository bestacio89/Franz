using Franz.Common.Business.Domain;
using Franz.Common.Mapping.Abstractions;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.DTOs;
using Franz.Contracts.Persistence;
using Franz.Contracts.Queries.Books;
using Franz.Domain.Entities;

namespace Franz.Application.Books.Queries;

public sealed class ListBooksQueryHandler
    : IQueryHandler<ListBooksQuery, Result<IReadOnlyCollection<BookDto>>>
{
  private readonly IBookRepository _bookRepository;
  private readonly IFranzMapper _mapper;

    public ListBooksQueryHandler(IBookRepository bookRepository, IFranzMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<BookDto>>> Handle(ListBooksQuery request, CancellationToken cancellationToken)
    {
        var books =  await _bookRepository.GetAllAsync(cancellationToken);
        var mapped = _mapper.Map<IReadOnlyCollection<Book>, IReadOnlyCollection<BookDto>>(books);

        return mapped.ToResult();
    }
}
