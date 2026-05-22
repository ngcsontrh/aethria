namespace Aethria.Application.UseCases.Resources.DownloadResource;

internal sealed class DownloadResourceQueryValidator : AbstractValidator<DownloadResourceQuery>
{
    public DownloadResourceQueryValidator()
    {
        RuleFor(query => query.ResourceId)
            .NotEmpty()
            .WithMessage("ResourceId is required.");

        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
