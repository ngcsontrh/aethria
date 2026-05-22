namespace Aethria.Application.UseCases.ChatSessions.DeleteAllChatSessions;

public sealed class DeleteAllChatSessionsCommandHandler : IRequestHandler<DeleteAllChatSessionsCommand, ValueTask<Result>>
{
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAllChatSessionsCommandHandler(
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(DeleteAllChatSessionsCommand command, CancellationToken cancellationToken)
    {
        switch (command.Scope)
        {
            case ChatSessionScope.General:
                await _chatSessionRepository.DeleteAllGeneralByUserIdAsync(command.UserId, cancellationToken);
                break;
            case ChatSessionScope.Mentor:
                await _chatSessionRepository.DeleteAllByMentorIdAsync(command.ScopeId!.Value, command.UserId, cancellationToken);
                break;
            case ChatSessionScope.Resource:
                await _chatSessionRepository.DeleteAllByResourceIdAsync(command.ScopeId!.Value, command.UserId, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Unsupported chat session scope '{command.Scope}'.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
