using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class LineEntityConfiguration : IEntityTypeConfiguration<LineEntity>
{
    public void Configure(EntityTypeBuilder<LineEntity> builder)
    {
        builder.OwnsMany(x => x.Points, points =>
        {
            points.ToJson();
        });
    }
}
