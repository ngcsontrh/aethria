using Aethria.Application.Extensions;
using FluentValidation.Results;
using System.Runtime.CompilerServices;

namespace Aethria.Application.Behaviors;

internal sealed class ValidationStreamPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IStreamRequest<TRequest, TResponse>
    where TResponse : ResultBase<TResponse>, new()
{
    public required IStreamRequestHandler<TRequest, TResponse> NextPipeline { get; set; }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var validationResult = await ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            yield return CreateFailureResult(validationResult);
            yield break;
        }

        await foreach (var result in NextPipeline.Handle(request, cancellationToken))
        {
            yield return result;
        }
    }

    private async ValueTask<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(validationResult.Errors);
        }

        return new ValidationResult(failures);
    }

    private static TResponse CreateFailureResult(ValidationResult validationResult)
    {
        var error = validationResult.ToValidationError();
        return new TResponse().WithError(error);
    }
}
