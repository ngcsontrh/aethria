using Aethria.Application.UseCases.Quizzes.Events;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Aethria.Application.UseCases.Quizzes.CreateAIQuizStream;

public sealed class CreateAIQuizStreamCommandHandler : IStreamRequestHandler<CreateAIQuizStreamCommand, Result<CreateAIQuizStreamEvent>>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IAIQuizGenerationWorkflow _workflow;
    private readonly ILogger<CreateAIQuizStreamCommandHandler> _logger;

    public CreateAIQuizStreamCommandHandler(
        IResourceRepository resourceRepository,
        IQuizRepository quizRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IAIQuizGenerationWorkflow workflow,
        ILogger<CreateAIQuizStreamCommandHandler> logger)
    {
        _resourceRepository = resourceRepository;
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _workflow = workflow;
        _logger = logger;
    }

    public async IAsyncEnumerable<Result<CreateAIQuizStreamEvent>> Handle(
        CreateAIQuizStreamCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, cancellationToken);
        if (resource is null || resource.UserId != command.UserId)
        {
            yield return Result.Fail<CreateAIQuizStreamEvent>(new NotFoundError("Resource not found."));
            yield break;
        }

        if (string.IsNullOrWhiteSpace(resource.Content))
        {
            yield return Result.Fail<CreateAIQuizStreamEvent>(
                new ValidationError("Resource has no content for AI quiz generation."));
            yield break;
        }

        var numberOfQuestions = command.NumberOfQuestions;
        CreateAIQuizStreamResult? completedResult = null;
        var input = new CreateAIQuizStreamInput
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
                yield return Result.Fail<CreateAIQuizStreamEvent>(
                    new InternalError(result.ErrorMessage ?? result.Message ?? "AI quiz generation failed."));
                yield break;
            }

            if (result.IsCompleted)
            {
                completedResult = result;
                break;
            }

            yield return Result.Ok(new CreateAIQuizStreamEvent(
                Status: result.Status,
                Message: result.Message ?? "AI quiz generation is in progress."));
        }

        if (completedResult is null)
        {
            yield return Result.Fail<CreateAIQuizStreamEvent>(new InternalError("AI quiz generation failed."));
            yield break;
        }

        if (completedResult.Questions.Count == 0)
        {
            yield return Result.Fail<CreateAIQuizStreamEvent>(new InternalError("AI generated no questions."));
            yield break;
        }

        if (completedResult.Questions.Count != numberOfQuestions)
        {
            _logger.LogWarning(
                "AI generated an unexpected number of questions. Expected {Expected}, actual {Actual}",
                numberOfQuestions,
                completedResult.Questions.Count);
            yield return Result.Fail<CreateAIQuizStreamEvent>(new InternalError("AI generated an invalid number of questions."));
            yield break;
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
                yield return Result.Fail<CreateAIQuizStreamEvent>(new InternalError("AI generated an invalid quiz question with out-of-bounds correct option index."));
                yield break;
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

        yield return Result.Ok(new CreateAIQuizStreamEvent(
            Status: CreateAIQuizStreamResult.Statuses.Completed,
            Message: "AI quiz generation completed.",
            QuizId: quizId));
    }
}
