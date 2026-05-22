namespace Aethria.Application.UseCases.ChatSessions.DeleteAllUserChatSessions;

public sealed class DeleteAllUserChatSessionsCommandHandler : IRequestHandler<DeleteAllUserChatSessionsCommand, ValueTask<Result>>
{
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAllUserChatSessionsCommandHandler(
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(DeleteAllUserChatSessionsCommand command, CancellationToken cancellationToken)
    {
        await _chatSessionRepository.DeleteAllByUserIdAsync(command.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
