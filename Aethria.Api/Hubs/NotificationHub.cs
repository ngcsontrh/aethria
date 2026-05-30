using Microsoft.AspNetCore.SignalR;

namespace Aethria.Api.Hubs;

/// <summary>
/// Pushes application notification events over SignalR.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
}
