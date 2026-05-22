using Aethria.Application.Extensions;
using FluentValidation.Results;

namespace Aethria.Application.Behaviors;

internal sealed class ValidationResultPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, ValueTask<TResponse>>
    where TRequest : class, IRequest<TRequest, ValueTask<TResponse>>
    where TResponse : ResultBase<TResponse>, new()
{
    public required IRequestHandler<TRequest, ValueTask<TResponse>> NextPipeline { get; set; }

    public async ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return CreateFailureResult(validationResult);
        }

        return await NextPipeline.Handle(request, cancellationToken);
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
