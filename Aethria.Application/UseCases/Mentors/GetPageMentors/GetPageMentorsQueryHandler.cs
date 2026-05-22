namespace Aethria.Application.UseCases.Mentors.GetPageMentors;

public sealed class GetPageMentorsQueryHandler : IRequestHandler<GetPageMentorsQuery, ValueTask<Result<PagedResponse<MentorPageItemResponse>>>>
{
    private readonly IMentorRepository _mentorRepository;

    public GetPageMentorsQueryHandler(
        IMentorRepository mentorRepository)
    {
        _mentorRepository = mentorRepository;
    }

    public async ValueTask<Result<PagedResponse<MentorPageItemResponse>>> Handle(GetPageMentorsQuery query, CancellationToken cancellationToken)
    {
        var (mentors, totalCount) = await _mentorRepository.GetPageByUserIdAsync(
            query.UserId, query.PageNumber, query.PageSize, cancellationToken);

        var items = mentors.Select(m => new MentorPageItemResponse(
            Id: m.Id,
            Name: m.Name,
            Description: m.Description,
            CreatedAt: m.CreatedAt,
            UpdatedAt: m.UpdatedAt
        )).ToList();

        return Result.Ok(new PagedResponse<MentorPageItemResponse>(items, totalCount, query.PageNumber, query.PageSize));
    }
}
