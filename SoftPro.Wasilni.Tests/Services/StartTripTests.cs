using Microsoft.Extensions.Caching.Memory;
using Moq;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Tests.Helpers;
using Xunit;

namespace SoftPro.Wasilni.Tests.Services;

/// <summary>
/// Tests for TripService.StartTripAsync
/// Covers: success, bus-not-found, wrong driver, reconnect, bus already active.
/// </summary>
public class StartTripTests : TripServiceBase
{
    // ─── Success ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Success_CreatesNewTrip_SavesAndCachesDriverKey()
    {
        // Arrange
        var bus = MakeBus(driverId: 1, busId: 10);
        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);
        TripRepo.Setup(x => x.GetActiveTripByBusIdAsync(10, Ct)).ReturnsAsync((TripEntity?)null);
        TripRepo.Setup(x => x.AddAsync(It.IsAny<TripEntity>(), Ct)).Returns(Task.CompletedTask);

        // Act
        var result = await Service.StartTripAsync(busId: 10, driverId: 1, Ct);

        // Assert — trip created and persisted
        Assert.NotNull(result);
        Assert.Equal(10,                result.BusId);
        Assert.Equal(TripStatus.Active, result.Status);
        Assert.Equal(0,                 result.AnonymousCount);
        TripRepo.Verify(x => x.AddAsync(It.IsAny<TripEntity>(), Ct), Times.Once);
        Uow.Verify(x => x.CompleteAsync(Ct), Times.Once);

        // Assert — driver-trip key written to cache
        Assert.True(Cache.TryGetValue("driver-trip:1", out _));
    }

    [Fact]
    public async Task Success_ReturnsTripWithCorrectBusPlateAndLineName()
    {
        // Arrange
        var bus = MakeBus(driverId: 1, busId: 10, lineId: 5, plate: "DAM-777");
        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);
        TripRepo.Setup(x => x.GetActiveTripByBusIdAsync(10, Ct)).ReturnsAsync((TripEntity?)null);
        TripRepo.Setup(x => x.AddAsync(It.IsAny<TripEntity>(), Ct)).Returns(Task.CompletedTask);

        // Act
        var result = await Service.StartTripAsync(busId: 10, driverId: 1, Ct);

        // Assert
        Assert.Equal("DAM-777",    result.BusPlate);
        Assert.Equal("Test Line",  result.LineName);
        Assert.Equal(5,            result.LineId);
    }

    // ─── Bus not found ────────────────────────────────────────────────────────

    [Fact]
    public async Task BusNotFound_ThrowsNotFoundException()
    {
        // Arrange
        BusRepo.Setup(x => x.GetByIdWithLineAsync(99, Ct)).ReturnsAsync((BusEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.StartTripAsync(busId: 99, driverId: 1, Ct));
    }

    [Fact]
    public async Task BusNotFound_NeverCallsTripRepository()
    {
        // Arrange
        BusRepo.Setup(x => x.GetByIdWithLineAsync(99, Ct)).ReturnsAsync((BusEntity?)null);

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.StartTripAsync(busId: 99, driverId: 1, Ct));

        // Assert — no trip queries made
        TripRepo.Verify(x => x.GetActiveTripByBusIdAsync(It.IsAny<int>(), Ct), Times.Never);
        TripRepo.Verify(x => x.AddAsync(It.IsAny<TripEntity>(), Ct), Times.Never);
    }

    // ─── Wrong driver ─────────────────────────────────────────────────────────

    [Fact]
    public async Task NotAssignedDriver_ThrowsUnauthorizedException()
    {
        // Arrange — bus is assigned to driver 5, driver 1 tries to start it
        var bus = MakeBus(driverId: 5, busId: 10);
        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.StartTripAsync(busId: 10, driverId: 1, Ct));
    }

    [Fact]
    public async Task NotAssignedDriver_NeverChecksForActiveTrip()
    {
        // Arrange
        var bus = MakeBus(driverId: 5, busId: 10);
        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.StartTripAsync(busId: 10, driverId: 1, Ct));

        // Assert — gate check is early; no trip look-up happens
        TripRepo.Verify(x => x.GetActiveTripByBusIdAsync(It.IsAny<int>(), Ct), Times.Never);
    }

    // ─── Reconnect ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reconnect_SameDriver_ReturnsExistingTrip_NoNewRecord()
    {
        // Arrange — active trip already exists for this driver (e.g. app restart)
        var bus      = MakeBus(driverId: 1, busId: 10);
        var existing = MakeTrip(driverId: 1, busId: 10, tripId: 100);
        SetProp(existing, nameof(TripEntity.Bus), bus);

        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);
        TripRepo.Setup(x => x.GetActiveTripByBusIdAsync(10, Ct)).ReturnsAsync(existing);

        // Act
        var result = await Service.StartTripAsync(busId: 10, driverId: 1, Ct);

        // Assert — returns the same trip, nothing written
        Assert.Equal(100, result.Id);
        TripRepo.Verify(x => x.AddAsync(It.IsAny<TripEntity>(), Ct), Times.Never);
        Uow.Verify(x => x.CompleteAsync(Ct), Times.Never);
    }

    [Fact]
    public async Task Reconnect_SameDriver_RestoresDriverTripCacheKey()
    {
        // Arrange
        var bus      = MakeBus(driverId: 1, busId: 10);
        var existing = MakeTrip(driverId: 1, busId: 10, tripId: 100);
        SetProp(existing, nameof(TripEntity.Bus), bus);

        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);
        TripRepo.Setup(x => x.GetActiveTripByBusIdAsync(10, Ct)).ReturnsAsync(existing);

        // Act
        await Service.StartTripAsync(busId: 10, driverId: 1, Ct);

        // Assert — cache key restored with correct trip id
        Assert.True(Cache.TryGetValue("driver-trip:1", out int cachedId));
        Assert.Equal(100, cachedId);
    }

    [Fact]
    public async Task Reconnect_SameDriver_ReturnsLastKnownLocationFromCache()
    {
        // Arrange
        var bus      = MakeBus(driverId: 1, busId: 10);
        var existing = MakeTrip(driverId: 1, busId: 10, tripId: 100);
        SetProp(existing, nameof(TripEntity.Bus), bus);
        SetLocation(tripId: 100, lat: 33.5, lng: 36.3);   // location still in cache

        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);
        TripRepo.Setup(x => x.GetActiveTripByBusIdAsync(10, Ct)).ReturnsAsync(existing);

        // Act
        var result = await Service.StartTripAsync(busId: 10, driverId: 1, Ct);

        // Assert
        Assert.Equal(33.5, result.Latitude);
        Assert.Equal(36.3, result.Longitude);
    }

    // ─── Bus already active by another driver ─────────────────────────────────

    [Fact]
    public async Task BusActiveByAnotherDriver_ThrowsAlreadyExistsException()
    {
        // Arrange — bus is active under driver 99, driver 1 tries to start it
        var bus      = MakeBus(driverId: 1, busId: 10);
        var existing = MakeTrip(driverId: 99, busId: 10, tripId: 50);

        BusRepo.Setup(x => x.GetByIdWithLineAsync(10, Ct)).ReturnsAsync(bus);
        TripRepo.Setup(x => x.GetActiveTripByBusIdAsync(10, Ct)).ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<AlreadyExistsException>(() =>
            Service.StartTripAsync(busId: 10, driverId: 1, Ct));
    }
}
