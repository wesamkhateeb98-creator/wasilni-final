using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;


namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class BusEntityConfiguration : IEntityTypeConfiguration<BusEntity>
{
    public void Configure(EntityTypeBuilder<BusEntity> builder)
    {
        builder.HasOne(l => l.LineEntity)
            .WithMany(c => c.Buses)
            .HasForeignKey(l => l.LineId)
            .OnDelete(DeleteBehavior.Restrict);

        //builder.HasMany(b => b.Requests)
        //      .WithOne(r => r.Bus)
        //      .HasForeignKey(r => r.BusId)
        //      .OnDelete(DeleteBehavior.Restrict);

        //builder.Property(p => p.OwnerPercent).HasColumnType(Config.DecimalThreeDigit);

        //builder.HasMany(b => b.Trips)
        //    .WithOne(t => t.BusEntity)
        //    .HasForeignKey(t => t.BusId)
        //    .OnDelete(DeleteBehavior.Restrict);

    }
}
