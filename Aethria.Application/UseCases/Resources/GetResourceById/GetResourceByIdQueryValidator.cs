namespace Aethria.Application.UseCases.Resources.GetResourceById;

internal sealed class GetResourceByIdQueryValidator : AbstractValidator<GetResourceByIdQuery>
{
    public GetResourceByIdQueryValidator()
    {
        RuleFor(query => query.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
