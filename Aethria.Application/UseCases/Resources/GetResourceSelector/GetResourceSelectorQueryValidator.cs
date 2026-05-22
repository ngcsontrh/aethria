namespace Aethria.Application.UseCases.Resources.GetResourceSelector;

internal sealed class GetResourceSelectorQueryValidator : AbstractValidator<GetResourceSelectorQuery>
{
    public GetResourceSelectorQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
