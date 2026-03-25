using Microsoft.Extensions.Caching.Memory;
using Moq;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Application.Services;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Trips;
using System.Reflection;

namespace SoftPro.Wasilni.Tests.Helpers;

/// <summary>
/// Shared base for all TripService test classes.
/// Each derived class gets a fresh set of mocks and a real MemoryCache.
/// </summary>
public abstract class TripServiceBase : IDisposable
{
    protected readonly Mock<IUnitOfWork>     Uow;
    protected readonly Mock<IBusRepository>  BusRepo;
    protected readonly Mock<ITripRepository> TripRepo;
    protected readonly IMemoryCache          Cache;
    protected readonly TripService           Service;

    protected static readonly CancellationToken Ct = CancellationToken.None;

    protected TripServiceBase()
    {
        Uow      = new Mock<IUnitOfWork>();
        BusRepo  = new Mock<IBusRepository>();
        TripRepo = new Mock<ITripRepository>();
        Cache    = new MemoryCache(new MemoryCacheOptions());

        Uow.Setup(x => x.BusRepository).Returns(BusRepo.Object);
        Uow.Setup(x => x.TripRepository).Returns(TripRepo.Object);
        Uow.Setup(x => x.CompleteAsync(Ct)).Returns(Task.CompletedTask);

        Service = new TripService(Uow.Object, Cache);
    }

    public void Dispose() => Cache.Dispose();

    // ─── Entity builders ──────────────────────────────────────────────────────

    /// <summary>Sets any property (including private-set) via reflection.</summary>
    protected static void SetProp(object obj, string propName, object? value)
    {
        var type = obj.GetType();
        PropertyInfo? prop = null;
        while (prop is null && type is not null)
        {
            prop = type.GetProperty(
                propName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.DeclaredOnly);
            type = type.BaseType;
        }
        prop!.SetValue(obj, value);
    }

    /// <summary>Creates a LineEntity via its private (string name) constructor.</summary>
    protected static LineEntity MakeLine(int id = 5, string name = "Test Line")
    {
        var ctor = typeof(LineEntity).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: [typeof(string)],
            modifiers: null)!;

        var line = (LineEntity)ctor.Invoke([name]);
        line.Id = id;
        return line;
    }

    /// <summary>Creates a BusEntity with its LineEntity navigation property populated.</summary>
    protected static BusEntity MakeBus(
        int    driverId,
        int    busId  = 10,
        int    lineId = 5,
        string plate  = "SYR-001")
    {
        var bus = new BusEntity(plate, "Blue", lineId, BusType.Bolman, 30, ownId: 1, driverId: driverId);
        bus.Id = busId;
        SetProp(bus, nameof(BusEntity.LineEntity), MakeLine(lineId));
        return bus;
    }

    /// <summary>Creates a TripEntity via TripEntity.Create factory.</summary>
    protected static TripEntity MakeTrip(
        int driverId = 1,
        int busId    = 10,
        int lineId   = 5,
        int tripId   = 100)
    {
        var trip = TripEntity.Create(busId, driverId, lineId);
        trip.Id = tripId;
        return trip;
    }

    /// <summary>Pre-populates cache with a known bus location.</summary>
    protected void SetLocation(int tripId, double lat = 33.5138, double lng = 36.2765)
        => Cache.Set($"bus-location:{tripId}", new BusLocationModel(lat, lng, DateTime.UtcNow));

    /// <summary>Pre-populates cache with the driver-trip mapping.</summary>
    protected void SetDriverTrip(int driverId, int tripId)
        => Cache.Set($"driver-trip:{driverId}", tripId);
}
