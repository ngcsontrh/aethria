namespace Aethria.Application.UseCases.Mentors.GetPageMentors;

public sealed record MentorPageItemResponse(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
