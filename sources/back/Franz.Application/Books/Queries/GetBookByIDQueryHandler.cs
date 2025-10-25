

// Queries/Books/Handlers/GetBookByIdQueryHandler.cs
using Franz.Common.Mediator;
using Franz.Common.Errors;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Common.Mapping.Abstractions;
using Franz.Domain.Entities;
using Franz.Common.Business.Domain;
using Franz.Contracts.DTOs;
using Franz.Contracts.Queries.Books;

namespace Franz.Application.Books.Queries
{
    public sealed class GetBookByIdQueryHandler
    : IQueryHandler<GetBookByIdQuery, Result<BookDto>>
    {
        private readonly IReadRepository<Book> _bookRepository;
    private readonly IFranzMapper _mapper;

        public GetBookByIdQueryHandler(IReadRepository<Book> bookRepository, IFranzMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<Result<BookDto>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetEntity(request.BookId);

            if (book is null)
                return "Book not found".ToFailure<BookDto>();

            return _mapper.Map<Book,BookDto>(book).ToResult();
        }
    }

}
