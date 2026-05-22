namespace Aethria.Infrastructure.Persistence.Configurations;

internal class RoadmapConfiguration : IEntityTypeConfiguration<Roadmap>
{
    public void Configure(EntityTypeBuilder<Roadmap> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired(false);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Mermaid)
            .IsRequired(false);

        builder.Property(x => x.ResourceId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ResourceId);
    }
}
