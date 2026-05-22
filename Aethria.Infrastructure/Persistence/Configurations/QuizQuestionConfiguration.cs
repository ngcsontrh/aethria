namespace Aethria.Infrastructure.Persistence.Configurations;

internal class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizId)
            .IsRequired();

        builder.Property(x => x.Text)
            .IsRequired();

        builder.Property(x => x.CorrectOptionId)
            .IsRequired();

        builder.Property(x => x.Explanation)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.QuizId);
    }
}
