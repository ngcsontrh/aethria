using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Aethria.Infrastructure.Persistence.Configurations;

internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Data)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>(),
                new ValueComparer<Dictionary<string, string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.OrderBy(kv => kv.Key).SequenceEqual(c2.OrderBy(kv => kv.Key)),
                    c => c.OrderBy(kv => kv.Key).Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                    c => c.ToDictionary(kv => kv.Key, kv => kv.Value)))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt })
            .IsDescending(false, false, true);
    }
}
