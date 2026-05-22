namespace Aethria.Application.UseCases.Resources.GetResourceById;

public sealed record GetResourceByIdResponse(
    Guid Id,
    string Name,
    string? Description,
    string FileName,
    string DownloadUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
