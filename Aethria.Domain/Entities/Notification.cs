namespace Aethria.Domain.Entities;

public class Notification : AggregateRoot
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
}
