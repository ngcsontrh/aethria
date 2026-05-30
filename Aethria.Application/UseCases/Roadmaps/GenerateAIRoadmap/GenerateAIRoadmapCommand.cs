namespace Aethria.Application.UseCases.Roadmaps.GenerateAIRoadmap;

public sealed record GenerateAIRoadmapCommand(
    string Name,
    string? Description,
    Guid ResourceId,
    string? Prompt,
    Guid UserId) : IRequest<GenerateAIRoadmapCommand, ValueTask<Result<Guid>>>;
