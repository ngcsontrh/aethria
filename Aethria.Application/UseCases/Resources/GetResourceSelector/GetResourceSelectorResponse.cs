namespace Aethria.Application.UseCases.Resources.GetResourceSelector;

public sealed record ResourceSelectorResponse(Guid Id, string Name);

public sealed record GetResourceSelectorResponse(IReadOnlyList<ResourceSelectorResponse> Resources);
