namespace Aethria.Infrastructure.Persistence.Configurations;

internal class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(88);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => ApiKeyStatus.FromValue(v))
            .HasMaxLength(20);

        builder.Property(x => x.RevokedAt)
            .IsRequired(false);

        builder.Property(x => x.LastFourChars)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Foreign key to AppUser
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on TokenHash for authentication lookups
        builder.HasIndex(x => x.TokenHash)
            .HasDatabaseName("ix_api_keys_token_hash")
            .IsUnique();

        // Composite index on (UserId, Status) for listing and counting active keys
        builder.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("ix_api_keys_user_id_status");

        // Composite index on (ExpiresAt, Status) for cleanup function queries
        builder.HasIndex(x => new { x.ExpiresAt, x.Status })
            .HasDatabaseName("ix_api_keys_expires_at_status");
    }
}
