namespace Aethria.Application.UseCases.Mentors.DeleteMentor;

public sealed record DeleteMentorCommand(
    Guid MentorId,
    Guid UserId) : IRequest<DeleteMentorCommand, ValueTask<Result>>;
