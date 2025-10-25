using Franz.Domain.ValueObjects;
using Franz.Common.Business.Domain;
using Franz.Common.Mediator.Results;
using Franz.Common.Mediator.Errors;

namespace Franz.Domain.Entities
{
    public sealed class Book : Entity<int>, IEntity
    {
        public int Id { get; set; }
        public ISBN Isbn { get; private set; }
        public Title Title { get; private set; }
        public Author Author { get; private set; }

        public DateTime PublishedOn { get; private set; }
        public int CopiesAvailable { get; private set; }

        private Book() { } // EF Core

        public Book(ISBN isbn, Title title, Author author, DateTime publishedOn, int copiesAvailable = 1)
        {
       
            Isbn = isbn ?? throw new ArgumentNullException(nameof(isbn));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            PublishedOn = publishedOn;
            CopiesAvailable = copiesAvailable;
        }

        public void UpdateDetails(Title title, Author author, ISBN isbn, int copiesAvailable)
        {
            Title = title;
            Author = author;
            Isbn = isbn;
            CopiesAvailable = copiesAvailable;
        }

        public Result AddCopies(int count)
        {
            if (count <= 0)
                return Result.Failure(Error.Validation("Book", "Copies to add must be positive."));

            CopiesAvailable += count;
          
            return Result.Success();
        }

        public Result Borrow()
        {
            if (CopiesAvailable <= 0)
                return Result.Failure(Error.Validation("Book", "No copies available."));

            CopiesAvailable--;
            
            return Result.Success();
        }

        public Result Return()
        {
            CopiesAvailable++;
         
            return Result.Success();
        }

    
    }
}
