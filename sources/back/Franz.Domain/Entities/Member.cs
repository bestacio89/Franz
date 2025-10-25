using Franz.Domain.ValueObjects;
using Franz.Common.Business.Domain;
using Franz.Common.Mediator.Results;
using Franz.Common.Mediator.Errors;

namespace Franz.Domain.Entities
{
    public sealed class Member : Entity<int>, IEntity
    {
        public FullName Name { get; private set; }
        public Email Email { get; private set; }

        private readonly List<int> _borrowedBooks = new();
        public IReadOnlyCollection<int> BorrowedBooks => _borrowedBooks.AsReadOnly();

        private Member() { } // EF Core

        public Member(FullName name, Email email)
        {
            

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }

        public void UpdateDetails(FullName name, Email email)
        {
            Name = name;
            Email = email;
        }
        public Result BorrowBook(Book book)
        {
            if (book is null)
                return Result.Failure(Error.Validation("Book", "Book cannot be null."));

            var borrowResult = book.Borrow();
            if (borrowResult.IsSuccess)
                return borrowResult;

            _borrowedBooks.Add(book.Id);
          
            return Result.Success();
        }

        public Result ReturnBook(Book book)
        {
            if (book is null)
                return Result.Failure(Error.Validation("Book", "Book cannot be null."));

            if (!_borrowedBooks.Contains(book.Id))
                return Result.Failure(Error.Validation("Book", "This member has not borrowed this book."));

            var returnResult = book.Return();
            if (returnResult.IsSuccess!)
                return returnResult;

            _borrowedBooks.Remove(book.Id);
         
            return Result.Success();
        }

      
    }
}
