namespace Aethria.Application.UseCases.Resources.CreateResource;

public sealed record CreateResourceCommand(
    string Name,
    string? Description,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize,
    Guid UserId) : IRequest<CreateResourceCommand, ValueTask<Result<Guid>>>;
