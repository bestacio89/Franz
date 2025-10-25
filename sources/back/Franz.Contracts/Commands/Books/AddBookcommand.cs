// Commands/Books/AddBookCommand.cs
using Franz.Common.Mediator;
using Franz.Common.Mediator.Messages;
namespace Franz.Contracts.Commands.Books;
public sealed record AddBookCommand(string Title, string Author, string Isbn, DateTime Datepublished, int Copies)
    : ICommand<int>;
