namespace Aethria.Application.UseCases.Resources.GetPageResources;

public sealed record ResourcePageItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string FileType,
    long FileSize,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
