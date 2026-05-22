namespace Aethria.Infrastructure.Persistence.Configurations;

internal class QuestionSnapshotConfiguration : IEntityTypeConfiguration<QuestionSnapshot>
{
    public void Configure(EntityTypeBuilder<QuestionSnapshot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizVersionId)
            .IsRequired();

        builder.Property(x => x.OriginalQuestionId)
            .IsRequired();

        builder.Property(x => x.Text)
            .IsRequired();

        builder.Property(x => x.Explanation)
            .IsRequired();

        builder.Property(x => x.CorrectOptionId)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.QuestionSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.QuizVersionId);
    }
}
