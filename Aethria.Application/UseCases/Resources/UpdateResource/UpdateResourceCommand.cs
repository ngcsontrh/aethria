namespace Aethria.Application.UseCases.Resources.UpdateResource;

public sealed record UpdateResourceCommand(Guid ResourceId, string Name, string? Description, Guid UserId) : IRequest<UpdateResourceCommand, ValueTask<Result>>;
