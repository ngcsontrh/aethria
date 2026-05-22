namespace Aethria.Infrastructure.Persistence.Configurations;

internal class ResourceChunkConfiguration : IEntityTypeConfiguration<ResourceChunk>
{
    public void Configure(EntityTypeBuilder<ResourceChunk> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ResourceId)
            .IsRequired();

        builder.Property(x => x.ChunkIndex)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Embedding)
            .HasColumnType("vector(1536)")
            .HasConversion(
                v => v.HasValue ? new Pgvector.Vector(v.Value) : null,
                v => v != null ? new ReadOnlyMemory<float>(v.ToArray()) : new ReadOnlyMemory<float>?()
            );

        builder.HasIndex(x => x.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");

        builder.HasIndex(x => x.ResourceId);
    }
}
