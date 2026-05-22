namespace Aethria.Domain.Entities;

public class ResourceChunk : BaseEntity
{
    public Guid ResourceId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = null!;

    public ReadOnlyMemory<float>? Embedding { get; set; }
}
