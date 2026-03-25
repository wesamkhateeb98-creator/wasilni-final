using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class PointEntityConfiguration : IEntityTypeConfiguration<PointEntity>
{
    public void Configure(EntityTypeBuilder<PointEntity> builder)
    {
        builder.HasOne(p => p.Line)
               .WithMany()
               .HasForeignKey(p => p.LineId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
