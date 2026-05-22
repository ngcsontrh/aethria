namespace Aethria.Infrastructure.Persistence.Configurations;

internal class QuizVersionConfiguration : IEntityTypeConfiguration<QuizVersion>
{
    public void Configure(EntityTypeBuilder<QuizVersion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuizId)
            .IsRequired();

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.HasMany(x => x.QuestionSnapshots)
            .WithOne()
            .HasForeignKey(x => x.QuizVersionId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasIndex(x => new { x.QuizId, x.VersionNumber })
            .IsUnique();
    }
}
