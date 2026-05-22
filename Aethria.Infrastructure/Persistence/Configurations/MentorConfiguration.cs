using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Aethria.Infrastructure.Persistence.Configurations;

internal class MentorConfiguration : IEntityTypeConfiguration<Mentor>
{
    public void Configure(EntityTypeBuilder<Mentor> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Instruction)
            .IsRequired();

        builder.Property(x => x.Tools)
            .HasConversion(
                v => JsonSerializer.Serialize(v.Select(t => t.Value), (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!.Select(t => MentorTool.FromValue(t)).ToList(),
                new ValueComparer<List<MentorTool>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
