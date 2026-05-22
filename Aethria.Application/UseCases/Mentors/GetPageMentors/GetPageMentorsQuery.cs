namespace Aethria.Application.UseCases.Mentors.GetPageMentors;

public sealed record GetPageMentorsQuery(
    Guid UserId,
    int PageNumber,
    int PageSize) : IRequest<GetPageMentorsQuery, ValueTask<Result<PagedResponse<MentorPageItemResponse>>>>;
