using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Franz.Contracts.Commands.Books
{
    public sealed record BorrowBookCommand(int MemberId, int BookId) : ICommand<Result>;
}
