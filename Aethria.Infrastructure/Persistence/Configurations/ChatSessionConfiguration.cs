namespace Aethria.Infrastructure.Persistence.Configurations;

internal class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MentorId)
            .IsRequired(false);

        builder.Property(x => x.ResourceId)
            .IsRequired(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ResourceId, x.UserId, x.UpdatedAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("ix_chat_sessions_resource_id_user_id_updated_at");

        builder.HasIndex(x => new { x.MentorId, x.UserId, x.UpdatedAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("ix_chat_sessions_mentor_id_user_id_updated_at");

        builder.HasIndex(x => new { x.UserId, x.UpdatedAt })
            .IsDescending(false, true)
            .HasFilter("mentor_id IS NULL AND resource_id IS NULL")
            .HasDatabaseName("ix_chat_sessions_general_user_id_updated_at");
    }
}
