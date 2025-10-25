using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Franz.Contracts.Commands.Books
{
    public sealed record UpdateBookCommand(int Id, string Title, string Author, string Isbn, int Copies)
    : ICommand<Result>;

}
