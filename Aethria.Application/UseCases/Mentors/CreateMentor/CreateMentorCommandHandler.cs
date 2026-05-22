namespace Aethria.Application.UseCases.Mentors.CreateMentor;

public sealed class CreateMentorCommandHandler : IRequestHandler<CreateMentorCommand, ValueTask<Result<Guid>>>
{
    private static readonly TimeSpan MentorValidationTimeout = TimeSpan.FromSeconds(30);
    private readonly IMentorRepository _mentorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMentorValidatorAgent _mentorValidatorAgent;

    public CreateMentorCommandHandler(
        IMentorRepository mentorRepository,
        IUnitOfWork unitOfWork,
        IMentorValidatorAgent mentorValidatorAgent)
    {
        _mentorRepository = mentorRepository;
        _unitOfWork = unitOfWork;
        _mentorValidatorAgent = mentorValidatorAgent;
    }

    public async ValueTask<Result<Guid>> Handle(CreateMentorCommand command, CancellationToken cancellationToken)
    {
        var mentorInstructionValidation = await ValidateMentorInstructionAsync(command.Instruction, cancellationToken);
        if (mentorInstructionValidation.IsFailed)
        {
            return Result.Fail<Guid>(mentorInstructionValidation.Errors);
        }

        var now = DateTimeOffset.UtcNow;

        var mentor = new Mentor
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Name = command.Name,
            Description = command.Description,
            Instruction = command.Instruction,
            Tools = command.Tools?.Select(t => MentorTool.FromValue(t)).ToList() ?? [],
            CreatedAt = now,
            UpdatedAt = now
        };

        await _mentorRepository.AddAsync(mentor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(mentor.Id);
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
