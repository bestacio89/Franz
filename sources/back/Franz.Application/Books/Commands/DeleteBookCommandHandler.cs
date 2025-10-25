using Franz.Common.EntityFramework.Repositories;
using Franz.Common.Mediator.Errors;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.Commands.Books;
using Franz.Domain.Entities;
using Franz.Persistence;


namespace Franz.Application.Books.Commands
{
    public sealed class DeleteBookCommandHandler : ICommandHandler<DeleteBookCommand, Result>
    {
        private readonly EntityRepository<ApplicationDbContext, Book> _bookRepository;

        public DeleteBookCommandHandler(EntityRepository<ApplicationDbContext, Book> bookRepository) =>
            _bookRepository = bookRepository;

        public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);
            if (book is null)
                return Result.Failure(Error.NotFound("Book", request.Id));

            await _bookRepository.DeleteAsync(book, cancellationToken);
          

            return Result.Success();
        }
    }

}
