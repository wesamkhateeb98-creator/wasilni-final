using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Helper;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/seed")]
public class DevController(AppDbContext dbContext) : BaseController
{
    // ─── Constants ────────────────────────────────────────────────────────────

    private const string DefaultPassword = "Password@123";
    private const string AdminPassword   = "Admin@123";
    private const string DevFcm          = "dev-fcm-token";

    // ─── Fake Data ────────────────────────────────────────────────────────────

    private static readonly string[] DriverNames = ["Ahmad Khalil", "Majd Hasan", "Rami Nassar", "Tarek Yousef", "Omar Faris"];
    private static readonly string[] DriverPhones = ["0911000001", "0911000002", "0911000003", "0911000004", "0911000005"];

    private static readonly string[] PassengerNames =
    [
        "Sara Ali", "Lina Mrad", "Hala Zain", "Nour Hamdan", "Rima Saleh",
        "Yousef Issa", "Karim Diab", "Sami Naser", "Dina Qasem", "Rana Hijazi",
        "Firas Akil", "Maya Taha", "Ruba Saad", "Bilal Moussa", "Jana Harb"
    ];

    private static readonly (string Plate, string Color, BusType Type, int Seats)[] BusSpecs =
    [
        ("DAM-1001", "White",  BusType.Bolman, 50),
        ("DAM-1002", "Blue",   BusType.Van,    14),
        ("DAM-1003", "Silver", BusType.Bolman, 50),
        ("DAM-1004", "Yellow", BusType.Van,    14),
        ("DAM-1005", "White",  BusType.Servece, 7),
    ];

    // ─── Seed Endpoint ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
    {
        // ── 1. Delete everything (order matters for FK constraints) ──────────
        await dbContext.Trips.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Buses.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Accounts.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Lines.ExecuteDeleteAsync(cancellationToken);

        // ── 2. Line ───────────────────────────────────────────────────────────
        var line = LineEntity.Create(new AddLineModel("Main Line"));
        await dbContext.Lines.AddAsync(line, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 3. Admin ──────────────────────────────────────────────────────────
        var admin = CreateAccount("Admin", "0900000000", AdminPassword, Role.Admin, Permission.None);
        await dbContext.Accounts.AddAsync(admin, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 4. Drivers (5) ────────────────────────────────────────────────────
        var drivers = new List<AccountEntity>();
        for (int i = 0; i < 5; i++)
        {
            var driver = CreateAccount(DriverNames[i], DriverPhones[i], DefaultPassword, Role.Passenger, Permission.BusDriving);
            drivers.Add(driver);
        }
        await dbContext.Accounts.AddRangeAsync(drivers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 5. Passengers (15) ────────────────────────────────────────────────
        var passengers = new List<AccountEntity>();
        for (int i = 0; i < 15; i++)
        {
            var p = CreateAccount(PassengerNames[i], $"0912{i + 1:D6}", DefaultPassword, Role.Passenger, Permission.None);
            passengers.Add(p);
        }
        await dbContext.Accounts.AddRangeAsync(passengers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 6. Buses (5) — each assigned to one driver ────────────────────────
        var buses = new List<BusEntity>();
        for (int i = 0; i < 5; i++)
        {
            var (plate, color, type, seats) = BusSpecs[i];
            var bus = new BusEntity(
                plate        : plate,
                color        : color,
                lineId       : line.Id,
                type         : type,
                numberOfSeats: seats,
                ownId        : admin.Id,
                driverId     : drivers[i].Id);
            buses.Add(bus);
        }
        await dbContext.Buses.AddRangeAsync(buses, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── 7. Response ───────────────────────────────────────────────────────
        return Ok(new
        {
            summary = new
            {
                line       = 1,
                admin      = 1,
                drivers    = drivers.Count,
                passengers = passengers.Count,
                buses      = buses.Count
            },
            credentials = new
            {
                adminPassword     = AdminPassword,
                defaultPassword   = DefaultPassword
            },
            line = new { id = line.Id, name = line.Name },

            admin = new
            {
                id       = admin.Id,
                name     = admin.Name,
                phone    = admin.PhoneNumber,
                password = AdminPassword,
                role     = admin.Role.ToString()
            },

            drivers = drivers.Select((d, i) => new
            {
                id         = d.Id,
                name       = d.Name,
                phone      = d.PhoneNumber,
                password   = DefaultPassword,
                permission = d.Permission.ToString(),
                busId      = buses[i].Id,
                busPlate   = buses[i].Plate
            }),

            passengers = passengers.Select(p => new
            {
                id    = p.Id,
                name  = p.Name,
                phone = p.PhoneNumber,
                password = DefaultPassword
            }),

            buses = buses.Select((b, i) => new
            {
                id           = b.Id,
                plate        = b.Plate,
                color        = b.Color,
                type         = b.Type.ToString(),
                numberOfSeats= b.NumberOfSeats,
                lineId       = b.LineId,
                driverId     = b.DriverId,
                driverName   = drivers[i].Name,
                driverPhone  = drivers[i].PhoneNumber
            })
        });
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private static AccountEntity CreateAccount(
        string     name,
        string     phone,
        string     password,
        Role       role,
        Permission permission)
    {
        byte[] salt = AuthHelper.GenerateSalt();
        var model   = new RegisterModel(
            Username  : name,
            Phonenumber: phone,
            Password  : password,
            FCMToken  : DevFcm,
            Role      : role);

        var account = AccountEntity.Create(
            model,
            AuthHelper.HashPasswordWithSalt(password, salt),
            salt,
            AuthHelper.GenerateRefreshToken(),
            "000000");

        account.ConfirmAccount();
        account.ChangePermission(permission);
        return account;
    }
}
