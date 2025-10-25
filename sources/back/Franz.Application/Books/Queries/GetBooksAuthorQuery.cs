using Franz.Common.Errors;
using Franz.Common.Mapping.Abstractions;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.DTOs;
using Franz.Contracts.Persistence;
using Franz.Contracts.Queries.Books;
using Franz.Domain.Entities;


namespace Franz.Application.Books.Queries
{
    public sealed class GetBooksByAuthorQueryHandler : IQueryHandler<GetBooksByAuthorQuery, Result<IEnumerable<BookDto>>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IFranzMapper _mapper;

        public GetBooksByAuthorQueryHandler(IBookRepository bookRepository, IFranzMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<BookDto>>> Handle(GetBooksByAuthorQuery request, CancellationToken cancellationToken)
        {
          var books = await _bookRepository.GetBooksByAuthorAsync(request.Author, cancellationToken);
          switch (request.Author)
          {
              case "Monster":
              throw TestExceptions.Monster();

              case "Banana":
              throw TestExceptions.BananaRepublic();

              case "Friendly":
              await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
              throw new TimeoutException("💌 Friendly Reminder: simulated timeout.");

              default:
              break;
          }

          if (!books.Any())
                return "No books found for this author".ToFailure<IEnumerable<BookDto>>();

            return _mapper.Map<IEnumerable<Book>,IEnumerable<BookDto>>(books).ToResult();
        }
    }
}

