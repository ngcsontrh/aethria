namespace Aethria.Infrastructure.Persistence.Configurations;

internal class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("roles");

        builder.HasIndex(r => r.NormalizedName).HasDatabaseName("ix_roles_normalized_name").IsUnique();
    }
}
