using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity>        Accounts        { get; set; } = null!;
    public DbSet<BusEntity>            Buses           { get; set; } = null!;
    public DbSet<LineEntity>           Lines           { get; set; } = null!;
    public DbSet<BookingEntity>        Bookings        { get; set; } = null!;
    public DbSet<DailyRidershipEntity> DailyRiderships { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
