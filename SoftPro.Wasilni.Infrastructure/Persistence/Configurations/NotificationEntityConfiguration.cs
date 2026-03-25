using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence.Configurations;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
       builder
            .HasOne(n => n.Sender)
            .WithMany()
            .HasForeignKey(n => n.SenderId)
            .OnDelete(DeleteBehavior.Restrict); // أو NoAction

        builder
            .HasOne(n => n.Receiver)
            .WithMany()
            .HasForeignKey(n => n.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict); // أو NoAction
    }
}
