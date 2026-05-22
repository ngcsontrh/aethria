namespace Aethria.Application.UseCases.Mentors.GetMentorById;

public sealed record GetMentorByIdQuery(
    Guid MentorId,
    Guid UserId) : IRequest<GetMentorByIdQuery, ValueTask<Result<GetMentorByIdResponse>>>;
