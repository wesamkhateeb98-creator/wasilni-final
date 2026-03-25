using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class TripEntityConfiguration : IEntityTypeConfiguration<TripEntity>
{
    public void Configure(EntityTypeBuilder<TripEntity> builder)
    {
        builder.HasOne(t => t.Bus)
               .WithMany()
               .HasForeignKey(t => t.BusId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
