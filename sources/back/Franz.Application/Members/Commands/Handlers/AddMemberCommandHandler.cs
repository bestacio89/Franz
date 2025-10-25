// Application/Members/Commands/AddMemberCommandHandler.cs

using Franz.Domain.Entities;
using Franz.Domain.ValueObjects;

using Franz.Common.Mapping.Abstractions;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Common.EntityFramework.Repositories;
using Franz.Persistence;
using Franz.Contracts.Commands.Members;

namespace Franz.Application.Members.Commands.Handlers;

public sealed class AddMemberCommandHandler
    : ICommandHandler<AddMemberCommand, Result<int>>
{
    private readonly EntityRepository<ApplicationDbContext,Member> _memberRepository;
    private readonly IFranzMapper _mapper;

    public AddMemberCommandHandler(EntityRepository<ApplicationDbContext, Member> memberRepository, IFranzMapper mapper)
    {
        _memberRepository = memberRepository;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(AddMemberCommand request, CancellationToken cancellationToken)
    {
        var member = new Member( new FullName (request.FullName), new Email(request.Email));

        await _memberRepository.AddAsync(member, cancellationToken);
        return member.Id.ToResult();
    }
}
