namespace Aethria.Application.UseCases.Resources.GetResourceSelector;

public sealed record GetResourceSelectorQuery(Guid UserId) : IRequest<GetResourceSelectorQuery, ValueTask<Result<GetResourceSelectorResponse>>>;
