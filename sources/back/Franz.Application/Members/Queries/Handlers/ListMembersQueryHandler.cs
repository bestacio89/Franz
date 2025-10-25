

using Franz.Common.Business.Domain;
using Franz.Common.Mapping.Abstractions;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.DTOs;
using Franz.Domain.Entities;

using Franz.Contracts.Queries.Members;

namespace Franz.Application.Members.Queries;

public sealed class ListMembersQueryHandler
    : IQueryHandler<ListMembersQuery, Result<IReadOnlyCollection<MemberDto>>>
{
    private readonly IReadRepository<Member> _memberRepository;
    private readonly IFranzMapper _mapper;

    public ListMembersQueryHandler(IReadRepository<Member> memberRepository, IFranzMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<MemberDto>>> Handle(ListMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _memberRepository.GetAll(cancellationToken);
        var mapped = _mapper.Map<IReadOnlyCollection<Member>,IReadOnlyCollection<MemberDto>>((IReadOnlyCollection<Member>)members);

        return mapped.ToResult();
    }
}
