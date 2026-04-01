using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Helper;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/seed")]
public class DevController(AppDbContext dbContext) : BaseController
{
    private const string DefaultPassword = "Password@123";
    private const string AdminPassword   = "Admin@123";
    private const string DevFcm          = "dev-fcm-token";

    private static readonly string[] Colors = ["White", "Blue", "Silver", "Yellow", "Red", "Green", "Orange", "Black", "Gray", "Beige"];
    private static readonly BusType[] BusTypes = [BusType.Bolman, BusType.Van, BusType.Servece];

    [HttpGet]
    public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
    {
        var rng = new Random(42);

        // ── 1. Clear (FK order) ───────────────────────────────────────────────
        await dbContext.Bookings.ExecuteDeleteAsync(cancellationToken);
        await dbContext.DailyRiderships.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Buses.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Accounts.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Lines.ExecuteDeleteAsync(cancellationToken);

        // ── 2. Admin ──────────────────────────────────────────────────────────
        var admin = CreateAccount("Admin", "0900000000", AdminPassword, Role.Admin);
        await dbContext.Accounts.AddAsync(admin, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 3. Lines (20) ─────────────────────────────────────────────────────
        var lines = new List<LineEntity>();
        for (int i = 1; i <= 20; i++)
        {
            lines.Add(LineEntity.Create(new AddLineModel($"خط {i}",
            [
                new(33.9668, 36.6572, 1),
                new(33.9675, 36.6585, 2),
                new(33.9682, 36.6600, 3),
                new(33.9695, 36.6615, 4),
                new(33.9710, 36.6630, 5),
                new(33.9725, 36.6645, 6),
            ])));
        }
        await dbContext.Lines.AddRangeAsync(lines, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 4. Drivers (200) + Buses (200) ────────────────────────────────────
        var drivers = new List<AccountEntity>();
        var buses   = new List<BusEntity>();

        for (int i = 1; i <= 200; i++)
        {
            var driver = CreateAccount($"Driver {i}", $"0911{i:D6}", DefaultPassword, Role.Passenger);
            drivers.Add(driver);
        }
        await dbContext.Accounts.AddRangeAsync(drivers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        for (int i = 0; i < 200; i++)
        {
            int lineIndex = i % 20; // 10 buses per line
            var bus = BusEntity.Create(new AddBusModel(
                Plate  : $"BUS-{i + 1:D4}",
                Color  : Colors[rng.Next(Colors.Length)],
                LineId : lines[lineIndex].Id,
                Type   : BusTypes[rng.Next(BusTypes.Length)]));
            bus.AssignDriverId(drivers[i].Id);
            buses.Add(bus);
        }
        await dbContext.Buses.AddRangeAsync(buses, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 5. Passengers (10) ───────────────────────────────────────────────
        var passengers = new List<AccountEntity>();
        for (int i = 1; i <= 10; i++)
        {
            passengers.Add(CreateAccount($"Passenger {i}", $"0912{i:D6}", DefaultPassword, Role.Passenger));
        }
        await dbContext.Accounts.AddRangeAsync(passengers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 6. DailyRidership — 200 buses × 100 days ─────────────────────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var riderships = (
            from d    in Enumerable.Range(0, 100)
            from bus  in buses
            select DailyRidershipEntity.Create(bus.LineId!.Value, bus.Id, today.AddDays(-d))
        ).ToList();

        await dbContext.DailyRiderships.AddRangeAsync(riderships, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Randomise NumberOfRiders (5–81) — bypasses private setter via SQL
        await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE DailyRiderships SET NumberOfRiders = ABS(CHECKSUM(NEWID())) % 77 + 5",
            cancellationToken);

        return Ok(new
        {
            summary = new
            {
                admin      = 1,
                lines      = lines.Count,
                drivers    = drivers.Count,
                passengers = passengers.Count,
                buses      = buses.Count,
                riderships = riderships.Count // 200 buses × 100 days
            },
            credentials = new { adminPassword = AdminPassword, defaultPassword = DefaultPassword },
            admin = new { id = admin.Id, phone = admin.PhoneNumber, password = AdminPassword }
        });
    }

    private static AccountEntity CreateAccount(string name, string phone, string password, Role role)
    {
        byte[] salt = AuthHelper.GenerateSalt();
        var account = AccountEntity.Create(
            new RegisterModel(name, phone, password, DevFcm, role),
            AuthHelper.HashPasswordWithSalt(password, salt),
            salt,
            AuthHelper.GenerateRefreshToken(),
            "000000");

        account.ConfirmAccount();
        return account;
    }
}
