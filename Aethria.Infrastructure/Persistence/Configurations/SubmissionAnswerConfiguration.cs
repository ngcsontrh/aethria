namespace Aethria.Infrastructure.Persistence.Configurations;

internal class SubmissionAnswerConfiguration : IEntityTypeConfiguration<SubmissionAnswer>
{
    public void Configure(EntityTypeBuilder<SubmissionAnswer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizSubmissionId)
            .IsRequired();

        builder.Property(x => x.QuestionSnapshotId)
            .IsRequired();

        builder.Property(x => x.SelectedOptionId)
            .IsRequired();

        builder.Property(x => x.IsCorrect)
            .IsRequired();


        builder.HasIndex(x => x.QuizSubmissionId);

        builder.HasIndex(x => x.QuestionSnapshotId); // Retained for query performance
    }
}
