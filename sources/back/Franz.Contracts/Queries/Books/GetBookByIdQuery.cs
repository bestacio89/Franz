using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Franz.Contracts.DTOs;

using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;
namespace Franz.Contracts.Queries.Books
{// Queries/Books/GetBookByIdQuery.cs


    public sealed record GetBookByIdQuery(int BookId) : IQuery<Result<BookDto>>;



}
