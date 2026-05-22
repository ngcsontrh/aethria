namespace Aethria.Application.UseCases.Mentors.GetMentorById;

public sealed class GetMentorByIdQueryHandler : IRequestHandler<GetMentorByIdQuery, ValueTask<Result<GetMentorByIdResponse>>>
{
    private readonly IMentorRepository _mentorRepository;

    public GetMentorByIdQueryHandler(
        IMentorRepository mentorRepository)
    {
        _mentorRepository = mentorRepository;
    }

    public async ValueTask<Result<GetMentorByIdResponse>> Handle(GetMentorByIdQuery query, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByIdAsync(query.MentorId, cancellationToken);

        if (mentor is null)
        {
            return Result.Fail(new NotFoundError("Mentor not found."));
        }

        if (mentor.UserId != query.UserId)
        {
            return Result.Fail(new NotFoundError("Mentor not found."));
        }

        var response = new GetMentorByIdResponse(
            Id: mentor.Id,
            UserId: mentor.UserId,
            Name: mentor.Name,
            Description: mentor.Description,
            Instruction: mentor.Instruction,
            Tools: [.. mentor.Tools.Select(t => t.Value)],
            CreatedAt: mentor.CreatedAt,
            UpdatedAt: mentor.UpdatedAt);

        return Result.Ok(response);
    }
}
