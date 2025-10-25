// Application/Members/Queries/ListMembersQuery.cs
using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;
using Franz.Contracts.DTOs;

namespace Franz.Contracts.Queries.Members;

public sealed record ListMembersQuery
    : IQuery<Result<IReadOnlyCollection<MemberDto>>>;
