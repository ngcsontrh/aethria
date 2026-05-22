namespace Aethria.Infrastructure.Persistence.Configurations;

internal class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizQuestionId)
            .IsRequired();

        builder.Property(x => x.Text)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasIndex(x => x.QuizQuestionId);
    }
}
