namespace Aethria.Infrastructure.Persistence.Configurations;

internal class QuestionOptionSnapshotConfiguration : IEntityTypeConfiguration<QuestionOptionSnapshot>
{
    public void Configure(EntityTypeBuilder<QuestionOptionSnapshot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionSnapshotId)
            .IsRequired();

        builder.Property(x => x.OriginalOptionId)
            .IsRequired();

        builder.Property(x => x.Text)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasIndex(x => x.QuestionSnapshotId);
    }
}
