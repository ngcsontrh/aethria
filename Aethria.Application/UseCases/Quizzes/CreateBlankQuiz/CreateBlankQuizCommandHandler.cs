namespace Aethria.Application.UseCases.Quizzes.CreateBlankQuiz;

public sealed class CreateBlankQuizCommandHandler : IRequestHandler<CreateBlankQuizCommand, ValueTask<Result<Guid>>>
{
    private readonly IQuizRepository _quizRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBlankQuizCommandHandler(
        IQuizRepository quizRepository,
        IResourceRepository resourceRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<Guid>> Handle(CreateBlankQuizCommand command, CancellationToken cancellationToken)
    {
        if (command.ResourceId.HasValue)
        {
            var resource = await _resourceRepository.GetByIdAsync(command.ResourceId.Value, cancellationToken);
            if (resource is null || resource.UserId != command.UserId)
            {
                return Result.Fail(new NotFoundError("Resource not found."));
            }
        }

        var now = DateTimeOffset.UtcNow;

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Name = command.Name,
            Description = command.Description,
            ResourceId = command.ResourceId,
            CurrentVersionNumber = 0,
            Questions = [],
            Versions = [],
            CreatedAt = now,
            UpdatedAt = now
        };

        await _quizRepository.AddAsync(quiz, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(quiz.Id);
    }
}
