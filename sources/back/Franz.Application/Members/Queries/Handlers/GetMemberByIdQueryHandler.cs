// Application/Members/Queries/GetMemberByIdQueryHandler.cs


using Franz.Domain.Entities;
using Franz.Common.Mapping.Abstractions;
using Franz.Common.Mediator.Errors;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Common.Business.Domain;
using Franz.Contracts.DTOs;
using Franz.Contracts.Queries.Members;

namespace Franz.Application.Members.Queries;

public sealed class GetMemberByIdQueryHandler
    : IQueryHandler<GetMemberByIdQuery, Result<MemberDto>>
{
    private readonly IReadRepository<Member>_memberRepository;
    private readonly IFranzMapper _mapper;

    public GetMemberByIdQueryHandler(IReadRepository<Member> memberRepository, IFranzMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<Result<MemberDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetEntity(request.MemberId);
        if (member is null)
            return Error.NotFound("Member", request.MemberId).ToFailure<MemberDto>();

        return _mapper.Map<Member, MemberDto>(member).ToResult();
    }
}
