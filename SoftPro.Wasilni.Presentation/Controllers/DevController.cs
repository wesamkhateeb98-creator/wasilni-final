
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Helper;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/seed")]
public class DevController(AppDbContext dbContext) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> SeedAsync(CancellationToken cancellationToken)
    {
        // Clear all existing data before re-seeding
        await dbContext.Buses.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Accounts.ExecuteDeleteAsync(cancellationToken);

        // 1. Create one line
        var line = LineEntity.Create(new SoftPro.Wasilni.Domain.Models.Lines.AddLineModel("Main Line"));
        await dbContext.Lines.AddAsync(line, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 2. Create 1 admin
        var accounts = new List<AccountEntity>();

        byte[] adminSalt = AuthHelper.GenerateSalt();
        var adminModel = new RegisterModel(
            Username: "Admin",
            Phonenumber: "0900000000",
            Password: "Admin@123",
            FCMToken: "dev-fcm-token",
            Role: Role.Admin);

        AccountEntity admin = AccountEntity.Create(
            adminModel,
            AuthHelper.HashPasswordWithSalt("Admin@123", adminSalt),
            adminSalt,
            AuthHelper.GenerateRefreshToken(),
            "000000");
        admin.ConfirmAccount();
        accounts.Add(admin);
        AccountEntity passenger;
        // 3. Create 20 passengers
        for (int i = 1; i <= 20; i++)
        {
            byte[] salt = AuthHelper.GenerateSalt();
            var model = new RegisterModel(
                Username: $"User {i}",
                Phonenumber: $"0901{i:D6}",
                Password: "Password@123",
                FCMToken: "dev-fcm-token",
                Role: Role.Passenger);

            AccountEntity account = AccountEntity.Create(
                model,
                AuthHelper.HashPasswordWithSalt("Password@123", salt),
                salt,
                AuthHelper.GenerateRefreshToken(),
                "000000");
            account.ConfirmAccount();
            accounts.Add(account);
            passenger = account;
        }

        await dbContext.Accounts.AddRangeAsync(accounts, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 4. Create 10 buses — assign passenger i as driver of bus i
        //    accounts[0] = Admin, accounts[1..10] = Users 1–10
        var buses = new List<BusEntity>();
        for (int i = 0; i < 10; i++)
        {
            AccountEntity driver = accounts[i + 1]; // skip admin at index 0
            var bus = new BusEntity(
                plate: $"WAS-{1000 + i}",
                color: "White",
                lineId: line.Id,
                type: BusType.Van,
                numberOfSeats: 14,
                ownId: driver.Id,
                driverId: driver.Id);

            buses.Add(bus);
        }

        await dbContext.Buses.AddRangeAsync(buses, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            summary = new
            {
                lines = 1,
                admin = 1,
                passengers = 20,
                buses = 10
            },
            data = new
            {
                line = new
                {
                    id = line.Id,
                    name = line.Name
                },
                admin = new
                {
                    id = admin.Id,
                    username = admin.Name,
                    phone = admin.PhoneNumber,
                    password = "Admin@123",
                    role = admin.Role.ToString()
                },
                passengers = accounts.Skip(1).Select((a, i) => new
                {
                    id = a.Id,
                    username = a.Name,
                    phone = a.PhoneNumber,
                    password = "Password@123",
                    role = a.Role.ToString()
                }),
                buses = buses.Select((b, i) => new
                {
                    id = b.Id,
                    plate = b.Plate,
                    color = b.Color,
                    type = b.Type.ToString(),
                    numberOfSeats = b.NumberOfSeats,
                    lineId = b.LineId,
                    driverId = b.DriverId,
                    driverUsername = accounts[i + 1].Name,
                    driverPhone = accounts[i + 1].PhoneNumber
                })
            }
        });
    }
}
