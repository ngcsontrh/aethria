namespace Aethria.Application.UseCases.Mentors.UpdateMentor;

public sealed class UpdateMentorCommandHandler : IRequestHandler<UpdateMentorCommand, ValueTask<Result>>
{
    private static readonly TimeSpan MentorValidationTimeout = TimeSpan.FromSeconds(30);
    private readonly IMentorRepository _mentorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMentorValidatorAgent _mentorValidatorAgent;

    public UpdateMentorCommandHandler(
        IMentorRepository mentorRepository,
        IUnitOfWork unitOfWork,
        IMentorValidatorAgent mentorValidatorAgent)
    {
        _mentorRepository = mentorRepository;
        _unitOfWork = unitOfWork;
        _mentorValidatorAgent = mentorValidatorAgent;
    }

    public async ValueTask<Result> Handle(UpdateMentorCommand command, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByIdAsync(command.MentorId, cancellationToken);

        if (mentor is null || mentor.UserId != command.UserId)
        {
            return Result.Fail(new NotFoundError("Mentor not found."));
        }

        var mentorInstructionValidation = await ValidateMentorInstructionAsync(command.Instruction, cancellationToken);
        if (mentorInstructionValidation.IsFailed)
        {
            return mentorInstructionValidation;
        }

        mentor.Name = command.Name;
        mentor.Description = command.Description;
        mentor.Instruction = command.Instruction;
        mentor.Tools = command.Tools?.Select(t => MentorTool.FromValue(t)).ToList() ?? [];
        mentor.UpdatedAt = DateTimeOffset.UtcNow;

        await _mentorRepository.UpdateAsync(mentor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async ValueTask<Result> ValidateMentorInstructionAsync(string instruction, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(MentorValidationTimeout);

            var result = await _mentorValidatorAgent.ValidateAsync(instruction, cts.Token);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(
                    string.IsNullOrWhiteSpace(result.Reason)
                        ? "Mentor instruction is invalid."
                        : result.Reason));
            }

            return Result.Ok();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Fail(new TimeoutError());
        }
        catch (HttpRequestException)
        {
            return Result.Fail(new ServiceUnavailableError());
        }
        catch (Exception)
        {
            return Result.Fail(new ServiceUnavailableError());
        }
    }
}
