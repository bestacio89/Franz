// Application/Members/Queries/GetMemberByIdQuery.cs
using Franz.Common.Mediator.Messages;
using Franz.Common.Mediator.Results;
using Franz.Contracts.DTOs;

namespace Franz.Contracts.Queries.Members;

public sealed record GetMemberByIdQuery(int MemberId)
    : IQuery<Result<MemberDto>>;
