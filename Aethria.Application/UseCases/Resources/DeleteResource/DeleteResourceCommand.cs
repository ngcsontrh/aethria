namespace Aethria.Application.UseCases.Resources.DeleteResource;

public sealed record DeleteResourceCommand(Guid ResourceId, Guid UserId) : IRequest<DeleteResourceCommand, ValueTask<Result>>;
