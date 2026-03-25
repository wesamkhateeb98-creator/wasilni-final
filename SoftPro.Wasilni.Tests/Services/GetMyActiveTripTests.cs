using Microsoft.Extensions.Caching.Memory;
using Moq;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Tests.Helpers;
using Xunit;

namespace SoftPro.Wasilni.Tests.Services;

/// <summary>
/// Tests for TripService.GetMyActiveTripAsync
/// Covers: no active trip, trip with cached location, trip without cache, metadata correctness.
/// </summary>
public class GetMyActiveTripTests : TripServiceBase
{
    // ─── No active trip ───────────────────────────────────────────────────────

    [Fact]
    public async Task NoActiveTrip_ReturnsNull()
    {
        // Arrange
        TripRepo.Setup(x => x.GetActiveTripByDriverIdAsync(1, Ct)).ReturnsAsync((TripEntity?)null);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task NoActiveTrip_DoesNotReadCache()
    {
        // Arrange — cache has a dangling key, but no DB trip
        SetLocation(tripId: 100);
        TripRepo.Setup(x => x.GetActiveTripByDriverIdAsync(1, Ct)).ReturnsAsync((TripEntity?)null);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Null(result);
    }

    // ─── Location present in cache ────────────────────────────────────────────

    [Fact]
    public async Task CachedLocation_ReturnsLatitude()
    {
        // Arrange
        var (trip, _) = SetupActiveTrip(tripId: 100);
        SetLocation(tripId: 100, lat: 33.5138, lng: 36.2765);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(33.5138, result!.Latitude);
    }

    [Fact]
    public async Task CachedLocation_ReturnsLongitude()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);
        SetLocation(tripId: 100, lat: 33.5138, lng: 36.2765);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(36.2765, result!.Longitude);
    }

    [Fact]
    public async Task CachedLocation_ExactCoordinatesPreserved()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);
        SetLocation(tripId: 100, lat: 33.5101, lng: 36.2901);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(33.5101, result!.Latitude);
        Assert.Equal(36.2901, result!.Longitude);
    }

    // ─── No location in cache ─────────────────────────────────────────────────

    [Fact]
    public async Task NoCachedLocation_LatitudeIsNull()
    {
        // Arrange — driver just started, hasn't sent any location yet
        SetupActiveTrip(tripId: 100);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Null(result!.Latitude);
    }

    [Fact]
    public async Task NoCachedLocation_LongitudeIsNull()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Null(result!.Longitude);
    }

    // ─── Trip metadata ────────────────────────────────────────────────────────

    [Fact]
    public async Task ReturnsCorrectTripId()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(100, result!.Id);
    }

    [Fact]
    public async Task ReturnsCorrectBusId()
    {
        // Arrange
        SetupActiveTrip(tripId: 100, busId: 10);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(10, result!.BusId);
    }

    [Fact]
    public async Task ReturnsCorrectBusPlate()
    {
        // Arrange
        SetupActiveTrip(tripId: 100, plate: "SYR-999");

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal("SYR-999", result!.BusPlate);
    }

    [Fact]
    public async Task ReturnsCorrectLineId()
    {
        // Arrange
        SetupActiveTrip(tripId: 100, lineId: 5);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(5, result!.LineId);
    }

    [Fact]
    public async Task ReturnsStatusActive()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(TripStatus.Active, result!.Status);
    }

    [Fact]
    public async Task ReturnsAnonymousCountZero_OnFreshTrip()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.Equal(0, result!.AnonymousCount);
    }

    [Fact]
    public async Task ReturnsStartedAt_NotDefault()
    {
        // Arrange
        SetupActiveTrip(tripId: 100);

        // Act
        var result = await Service.GetMyActiveTripAsync(driverId: 1, Ct);

        // Assert
        Assert.NotEqual(default, result!.StartedAt);
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    /// <summary>Wires up TripRepo to return a trip with Bus navigation populated.</summary>
    private (TripEntity trip, BusEntity bus) SetupActiveTrip(
        int    tripId  = 100,
        int    busId   = 10,
        int    lineId  = 5,
        string plate   = "SYR-001")
    {
        var bus  = MakeBus(driverId: 1, busId: busId, lineId: lineId, plate: plate);
        var trip = MakeTrip(driverId: 1, busId: busId, lineId: lineId, tripId: tripId);
        SetProp(trip, nameof(TripEntity.Bus), bus);

        TripRepo.Setup(x => x.GetActiveTripByDriverIdAsync(1, Ct)).ReturnsAsync(trip);
        return (trip, bus);
    }
}
