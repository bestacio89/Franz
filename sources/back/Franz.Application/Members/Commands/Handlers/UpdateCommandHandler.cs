// Application/Members/Commands/UpdateMemberCommandHandler.cs
using Franz.Common.EntityFramework.Repositories;
using Franz.Common.Mediator.Errors;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.Commands.Members;
using Franz.Domain.Entities;
using Franz.Domain.ValueObjects;
using Franz.Persistence;


namespace Franz.Application.Members.Commands.Handlers;

public sealed class UpdateMemberCommandHandler
    : ICommandHandler<UpdateMemberCommand, Result>
{
    private readonly EntityRepository<ApplicationDbContext, Member> _memberRepository;

    public UpdateMemberCommandHandler(EntityRepository<ApplicationDbContext, Member> memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
            return Error.NotFound("Member", request.MemberId).ToFailure<Result<Member>>();

        member.UpdateDetails(new FullName (request.FullName), new Email(request.Email));

        await _memberRepository.UpdateAsync(member, cancellationToken);

        return Result.Success();
    }
}
