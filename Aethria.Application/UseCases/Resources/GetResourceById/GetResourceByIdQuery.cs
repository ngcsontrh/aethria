namespace Aethria.Application.UseCases.Resources.GetResourceById;

public sealed record GetResourceByIdQuery(Guid ResourceId, Guid UserId) : IRequest<GetResourceByIdQuery, ValueTask<Result<GetResourceByIdResponse>>>;
