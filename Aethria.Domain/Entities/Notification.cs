namespace Aethria.Domain.Entities;

public class Notification : AggregateRoot
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = null!;
    public Dictionary<string, string> Data { get; set; } = [];
    public bool IsRead { get; set; }
}
