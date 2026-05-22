using Aethria.Api.Endpoints.Quizzes;
using Aethria.Application.UseCases.Quizzes.CreateBlankQuiz;
using Aethria.Application.UseCases.Quizzes.DeleteQuiz;
using Aethria.Application.UseCases.Quizzes.GetPageQuizzes;
using Aethria.Application.UseCases.Quizzes.GetQuizById;
using Aethria.Application.UseCases.Quizzes.GetQuizQuestions;
using Aethria.Application.UseCases.Quizzes.GetQuizQuestionsForEdit;
using Aethria.Application.UseCases.Quizzes.GetQuizSubmissionHistory;
using Aethria.Application.UseCases.Quizzes.GetSubmissionById;
using Aethria.Application.UseCases.Quizzes.SubmitQuizAnswers;
using Aethria.Application.UseCases.Quizzes.UpdateQuiz;

namespace Aethria.Api.Endpoints;

internal static class QuizEndpoints
{
    internal static void MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/quizzes")
            .RequireAuthorization()
            .WithTags("Quizzes");

        group.MapPost("blank", CreateBlank)
            .WithName("CreateBlankQuiz")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("", GetQuizzes)
            .WithName("GetQuizzes")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("{id:guid}", GetQuizById)
            .WithName("GetQuizById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/questions", GetQuizQuestions)
            .WithName("GetQuizQuestions")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/questions/edit", GetQuizQuestionsForEdit)
            .WithName("GetQuizQuestionsForEdit")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("{id:guid}/submissions", SubmitQuizAnswers)
            .WithName("SubmitQuizAnswers")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}", UpdateQuiz)
            .WithName("UpdateQuiz")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("{id:guid}", DeleteQuiz)
            .WithName("DeleteQuiz")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/submissions", GetQuizSubmissionHistory)
            .WithName("GetQuizSubmissionHistory")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{id:guid}/submissions/{submissionId:guid}", GetSubmissionById)
            .WithName("GetQuizSubmissionById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Create a new blank quiz with no questions.
    /// </summary>
    public static async Task<IResult> CreateBlank(
        [FromBody] CreateBlankQuizRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new CreateBlankQuizCommand(
            Name: request.Name,
            Description: request.Description,
            ResourceId: request.ResourceId,
            UserId: user.GetUserId());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Json(new { id = result.Value }, statusCode: StatusCodes.Status201Created);
    }

    /// <summary>
    /// Get a paginated list of quizzes for the current user.
    /// </summary>
    public static async Task<IResult> GetQuizzes(
        [AsParameters] GetPageQuizzesRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetPageQuizzesQuery(
            UserId: user.GetUserId(),
            PageNumber: request.PageNumber,
            PageSize: request.PageSize);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Get detailed information about a specific quiz.
    /// </summary>
    public static async Task<IResult> GetQuizById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetQuizByIdQuery(
            QuizId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Get the questions and answer options for a quiz (for quiz taking).
    /// </summary>
    public static async Task<IResult> GetQuizQuestions(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetQuizQuestionsQuery(
            QuizId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Get the questions, answer options, explanations, and correct answers for editing a quiz.
    /// </summary>
    public static async Task<IResult> GetQuizQuestionsForEdit(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetQuizQuestionsForEditQuery(
            QuizId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Submit answers for a quiz attempt.
    /// </summary>
    public static async Task<IResult> SubmitQuizAnswers(
        [FromRoute] Guid id,
        [FromBody] SubmitQuizRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new SubmitQuizAnswersCommand(
            QuizId: id,
            UserId: user.GetUserId(),
            QuizVersionId: request.QuizVersionId,
            Answers: [.. request.Answers.Select(a => new SubmitAnswerItem(a.QuestionSnapshotId, a.SelectedOptionId))]);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Json(result.Value, statusCode: StatusCodes.Status201Created);
    }

    /// <summary>
    /// Update a quiz's metadata and/or questions.
    /// </summary>
    public static async Task<IResult> UpdateQuiz(
        [FromRoute] Guid id,
        [FromBody] UpdateQuizRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuizCommand(
            QuizId: id,
            UserId: user.GetUserId(),
            Name: request.Name,
            Description: request.Description,
            Questions: request.Questions?.Select(q => new UpdateQuestionItem(
                Text: q.Text,
                Explanation: q.Explanation,
                OrderIndex: q.OrderIndex,
                Options: [.. q.Options.Select(o => new UpdateOptionItem(o.Text, o.OrderIndex))],
                CorrectOptionIndex: q.CorrectOptionIndex)).ToList());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Delete a quiz and all its related data.
    /// </summary>
    public static async Task<IResult> DeleteQuiz(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var command = new DeleteQuizCommand(
            QuizId: id,
            UserId: user.GetUserId());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.NoContent();
    }

    /// <summary>
    /// Get a paginated history of quiz submissions for a quiz.
    /// </summary>
    public static async Task<IResult> GetQuizSubmissionHistory(
        [FromRoute] Guid id,
        [AsParameters] GetQuizSubmissionHistoryRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetQuizSubmissionHistoryQuery(
            QuizId: id,
            UserId: user.GetUserId(),
            PageNumber: request.PageNumber,
            PageSize: request.PageSize);

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Get detailed information about a specific quiz submission.
    /// </summary>
    public static async Task<IResult> GetSubmissionById(
        [FromRoute] Guid id,
        [FromRoute] Guid submissionId,
        [FromServices] IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var query = new GetSubmissionByIdQuery(
            QuizId: id,
            SubmissionId: submissionId,
            UserId: user.GetUserId());

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToErrorResult();
        }

        return Results.Ok(result.Value);
    }
}
