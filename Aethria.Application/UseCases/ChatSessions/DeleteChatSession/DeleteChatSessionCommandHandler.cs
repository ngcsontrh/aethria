namespace Aethria.Application.UseCases.ChatSessions.DeleteChatSession;

public sealed class DeleteChatSessionCommandHandler : IRequestHandler<DeleteChatSessionCommand, ValueTask<Result>>
{
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteChatSessionCommandHandler(
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(DeleteChatSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await _chatSessionRepository.GetByIdAsync(command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Fail(new NotFoundError("ChatSession", command.SessionId.ToString()));
        }

        if (session.UserId != command.UserId || !SessionMatchesScope(session.MentorId, session.ResourceId, command.Scope, command.ScopeId))
        {
            return Result.Fail(new NotFoundError("ChatSession", command.SessionId.ToString()));
        }

        await _chatSessionRepository.DeleteAsync(command.SessionId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private static bool SessionMatchesScope(Guid? mentorId, Guid? resourceId, ChatSessionScope scope, Guid? scopeId)
    {
        return scope switch
        {
            ChatSessionScope.General => mentorId is null && resourceId is null,
            ChatSessionScope.Mentor => mentorId == scopeId,
            ChatSessionScope.Resource => resourceId == scopeId,
            _ => false
        };
    }
}
