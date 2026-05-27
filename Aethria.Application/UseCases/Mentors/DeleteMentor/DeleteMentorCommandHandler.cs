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
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _chatSessionRepository.DeleteAllByMentorIdAsync(mentor.Id, command.UserId, ct);
            await _mentorRepository.DeleteAsync(mentor.Id, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return Result.Ok();
    }
}
