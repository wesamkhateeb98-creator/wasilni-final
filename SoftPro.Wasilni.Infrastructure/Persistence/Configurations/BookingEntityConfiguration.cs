using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class BookingEntityConfiguration : IEntityTypeConfiguration<BookingEntity>
{
    public void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        builder.HasOne(b => b.Line)
               .WithMany()
               .HasForeignKey(b => b.LineId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Passenger)
               .WithMany()
               .HasForeignKey(b => b.PassengerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
