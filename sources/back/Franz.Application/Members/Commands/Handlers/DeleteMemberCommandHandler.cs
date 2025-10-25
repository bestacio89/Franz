// Application/Members/Commands/DeleteMemberCommandHandler.cs
using Franz.Common.EntityFramework.Repositories;
using Franz.Common.Mediator.Errors;
using Franz.Common.Mediator.Handlers;
using Franz.Common.Mediator.Results;
using Franz.Contracts.Commands.Members;
using Franz.Domain.Entities;
using Franz.Persistence;

namespace Franz.Application.Members.Commands;

public sealed class DeleteMemberCommandHandler
    : ICommandHandler<DeleteMemberCommand, Result>
{
    private readonly EntityRepository<ApplicationDbContext, Member> _memberRepository;

    public DeleteMemberCommandHandler(EntityRepository<ApplicationDbContext, Member> memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
            return Error.NotFound("Member", request.MemberId).ToFailure<Result>();

        await _memberRepository.DeleteAsync(member);
        
        return Result.Success();
    }
}
