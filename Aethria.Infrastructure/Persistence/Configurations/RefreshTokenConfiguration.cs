namespace Aethria.Infrastructure.Persistence.Configurations;

internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(88);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => RefreshTokenStatus.FromValue(v))
            .HasMaxLength(20);

        builder.Property(x => x.RevokedAt)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .HasDatabaseName("ix_refresh_tokens_token_hash")
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("ix_refresh_tokens_user_id_status");

        builder.HasIndex(x => new { x.ExpiresAt, x.Status })
            .HasDatabaseName("ix_refresh_tokens_expires_at_status");
    }
}
