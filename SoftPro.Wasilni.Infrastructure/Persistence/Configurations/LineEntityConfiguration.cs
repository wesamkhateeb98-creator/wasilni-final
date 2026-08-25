using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class LineEntityConfiguration : IEntityTypeConfiguration<LineEntity>
{
    public void Configure(EntityTypeBuilder<LineEntity> builder)
    {
        builder.Property(x => x.Points)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<Point>>(v, (JsonSerializerOptions)null) ?? new List<Point>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<List<Point>>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions)null),
                c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions)null).GetHashCode(),
                c => JsonSerializer.Deserialize<List<Point>>(JsonSerializer.Serialize(c, (JsonSerializerOptions)null), (JsonSerializerOptions)null)!));
    }
}