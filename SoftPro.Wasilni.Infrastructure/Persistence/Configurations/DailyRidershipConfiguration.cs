using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class DailyRidershipConfiguration : IEntityTypeConfiguration<DailyRidershipEntity>
{
    public void Configure(EntityTypeBuilder<DailyRidershipEntity> builder)
    {
        builder.Property(r => r.RowVersion)
               .IsRowVersion();

        builder.HasOne(r => r.Bus)
               .WithMany()
               .HasForeignKey(r => r.BusId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.BusId, r.Day })
               .IsUnique();
    }
}
