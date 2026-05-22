namespace Aethria.Application.UseCases.Mentors.GetMentorById;

public sealed record GetMentorByIdResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    string Instruction,
    List<string> Tools,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
