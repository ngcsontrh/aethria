namespace Aethria.Application.UseCases.Mentors.CreateMentor;

public sealed record CreateMentorCommand(
    string Name,
    string Description,
    string Instruction,
    List<string> Tools,
    Guid UserId) : IRequest<CreateMentorCommand, ValueTask<Result<Guid>>>;
