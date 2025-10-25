// Application/Members/Commands/AddMemberCommand.cs

using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;

namespace Franz.Contracts.Commands.Members;

public sealed record AddMemberCommand(string FullName, string Email)
    : ICommand<Result<int>>;
