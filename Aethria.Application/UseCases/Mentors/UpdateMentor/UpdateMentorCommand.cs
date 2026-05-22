namespace Aethria.Application.UseCases.Mentors.UpdateMentor;

public sealed record UpdateMentorCommand(
    Guid MentorId,
    string Name,
    string Description,
    string Instruction,
    List<string> Tools,
    Guid UserId) : IRequest<UpdateMentorCommand, ValueTask<Result>>;
