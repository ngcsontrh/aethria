using Aethria.Application.UseCases.Quizzes.Events;
using Microsoft.Extensions.Logging;

namespace Aethria.Application.UseCases.Quizzes.CreateAIQuiz;

public sealed class CreateAIQuizCommandHandler : IRequestHandler<CreateAIQuizCommand, ValueTask<Result<Guid>>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IAIQuizGenerationWorkflow _workflow;
    private readonly ILogger<CreateAIQuizCommandHandler> _logger;

    public CreateAIQuizCommandHandler(
        IResourceRepository resourceRepository,
        IQuizRepository quizRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IAIQuizGenerationWorkflow workflow,
        ILogger<CreateAIQuizCommandHandler> logger)
    {
        _resourceRepository = resourceRepository;
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _workflow = workflow;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(
        CreateAIQuizCommand command,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, cancellationToken);
        if (resource is null || resource.UserId != command.UserId)
        {
            return Result.Fail<Guid>(new NotFoundError("Resource not found."));
        }

        if (string.IsNullOrWhiteSpace(resource.Content))
        {
            const string message = "Resource has no content for AI quiz generation.";
            return Result.Fail<Guid>(new ValidationError(message));
        }

        var numberOfQuestions = command.NumberOfQuestions;
        CreateAIQuizResult? completedResult = null;
        var input = new CreateAIQuizInput
        {
            ResourceId = command.ResourceId,
            SourceContent = resource.Content,
            UserPrompt = command.Prompt,
            NumberOfQuestions = numberOfQuestions
        };

        await foreach (var result in _workflow.RunAsync(input, cancellationToken))
        {
            if (result.IsFailed)
            {
                var message = result.ErrorMessage ?? result.Message ?? "AI quiz generation failed.";
                return Result.Fail<Guid>(new InternalError(message));
            }

            if (result.IsCompleted)
            {
                completedResult = result;
                break;
            }

            _logger.LogDebug(
                "AI quiz generation progress for resource {ResourceId}: {Status} - {Message}",
                command.ResourceId,
                result.Status,
                result.Message);
        }

        if (completedResult is null)
        {
            const string message = "AI quiz generation failed.";
            return Result.Fail<Guid>(new InternalError(message));
        }

        if (completedResult.Questions.Count == 0)
        {
            const string message = "AI generated no questions.";
            return Result.Fail<Guid>(new InternalError(message));
        }

        if (completedResult.Questions.Count != numberOfQuestions)
        {
            _logger.LogWarning(
                "AI generated an unexpected number of questions. Expected {Expected}, actual {Actual}",
                numberOfQuestions,
                completedResult.Questions.Count);
            const string message = "AI generated an invalid number of questions.";
            return Result.Fail<Guid>(new InternalError(message));
        }

        var quizId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var questions = new List<QuizQuestion>();

        for (var i = 0; i < completedResult.Questions.Count; i++)
        {
            var generated = completedResult.Questions[i];
            var question = new QuizQuestion
            {
                Id = Guid.NewGuid(),
                Text = generated.QuestionText,
                Explanation = generated.Explanation,
                OrderIndex = i,
                Options = [],
                CreatedAt = now,
                UpdatedAt = now
            };

            for (var j = 0; j < generated.Options.Count; j++)
            {
                question.Options.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    QuizQuestionId = question.Id,
                    Text = generated.Options[j],
                    OrderIndex = j,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            if (generated.CorrectOptionIndex < 0 || generated.CorrectOptionIndex >= question.Options.Count)
            {
                const string message = "AI generated an invalid quiz question with out-of-bounds correct option index.";
                return Result.Fail<Guid>(new InternalError(message));
            }

            question.CorrectOptionId = question.Options.ElementAt(generated.CorrectOptionIndex).Id;
            questions.Add(question);
        }

        var quiz = new Quiz
        {
            Id = quizId,
            UserId = command.UserId,
            Name = command.Name,
            Description = command.Description,
            ResourceId = command.ResourceId,
            CurrentVersionNumber = 1,
            Questions = questions,
            Versions = [],
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var question in questions)
        {
            question.QuizId = quiz.Id;
        }

        var version = new QuizVersion
        {
            Id = Guid.NewGuid(),
            QuizId = quiz.Id,
            VersionNumber = 1,
            QuestionSnapshots = [],
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var question in questions)
        {
            var snapshot = new QuestionSnapshot
            {
                Id = Guid.NewGuid(),
                QuizVersionId = version.Id,
                OriginalQuestionId = question.Id,
                Text = question.Text,
                Explanation = question.Explanation,
                CorrectOptionId = question.CorrectOptionId,
                OrderIndex = question.OrderIndex,
                Options = [],
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var option in question.Options)
            {
                snapshot.Options.Add(new QuestionOptionSnapshot
                {
                    Id = Guid.NewGuid(),
                    QuestionSnapshotId = snapshot.Id,
                    OriginalOptionId = option.Id,
                    Text = option.Text,
                    OrderIndex = option.OrderIndex,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            snapshot.CorrectOptionId = snapshot.Options.First(o => o.OriginalOptionId == question.CorrectOptionId).Id;
            version.QuestionSnapshots.Add(snapshot);
        }

        quiz.Versions.Add(version);

        await _quizRepository.AddAsync(quiz, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var completedEvent = new AIQuizGenerationCompletedEvent(
            QuizId: quizId,
            UserId: command.UserId);

        await _mediator.Publish(completedEvent, cancellationToken);

        return Result.Ok(quizId);
    }
}
