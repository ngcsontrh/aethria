using DispatchR.Abstractions.Notification;

namespace Aethria.Application.UseCases.Roadmaps.Events;

public sealed record AIRoadmapGenerationCompletedEvent(
    Guid RoadmapId,
    Guid UserId) : INotification;
