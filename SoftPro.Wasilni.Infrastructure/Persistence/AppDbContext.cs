using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftPro.Wasilni.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts { get; set; } = null!;
    public DbSet<BusEntity> Buses { get; set; } = null!;
    public DbSet<LineEntity> Lines { get; set; } = null!;
    public DbSet<TripEntity> Trips { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
