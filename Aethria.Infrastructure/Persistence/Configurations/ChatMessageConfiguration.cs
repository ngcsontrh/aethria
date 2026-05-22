namespace Aethria.Infrastructure.Persistence.Configurations;

internal class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion(
                v => v.Value,
                v => ChatRole.FromValue(v))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.SessionId);
    }
}
