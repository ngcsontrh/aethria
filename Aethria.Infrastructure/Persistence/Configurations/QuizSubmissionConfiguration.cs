namespace Aethria.Infrastructure.Persistence.Configurations;

internal class QuizSubmissionConfiguration : IEntityTypeConfiguration<QuizSubmission>
{
    public void Configure(EntityTypeBuilder<QuizSubmission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.QuizId)
            .IsRequired();

        builder.Property(x => x.QuizVersionId)
            .IsRequired();

        builder.Property(x => x.Score)
            .IsRequired();

        builder.Property(x => x.TotalQuestions)
            .IsRequired();

        builder.Property(x => x.IsPassed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.Answers)
            .WithOne()
            .HasForeignKey(x => x.QuizSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_quiz_submissions_score_valid_range", "\"score\" >= 0 AND \"score\" <= \"total_questions\""));

        builder.HasIndex(x => x.QuizId);

        builder.HasIndex(x => x.QuizVersionId);
    }
}
