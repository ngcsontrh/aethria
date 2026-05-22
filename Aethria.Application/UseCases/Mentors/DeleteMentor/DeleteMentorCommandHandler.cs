namespace Aethria.Application.UseCases.Mentors.DeleteMentor;

public sealed class DeleteMentorCommandHandler : IRequestHandler<DeleteMentorCommand, ValueTask<Result>>
{
    private readonly IMentorRepository _mentorRepository;
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMentorCommandHandler(
        IMentorRepository mentorRepository,
        IChatSessionRepository chatSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _mentorRepository = mentorRepository;
        _chatSessionRepository = chatSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(DeleteMentorCommand command, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByIdAsync(command.MentorId, cancellationToken);

        if (mentor is null || mentor.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError("Mentor not found."));
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _chatSessionRepository.DeleteAllByMentorIdAsync(mentor.Id, command.UserId, cancellationToken);
            await _mentorRepository.DeleteAsync(mentor.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result.Ok();
    }
}
