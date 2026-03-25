using Moq;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Tests.Helpers;
using Xunit;

namespace SoftPro.Wasilni.Tests.Services;

/// <summary>
/// Tests for TripService.EndTripAsync
/// Covers: success (status + EndedAt + cache cleanup), not-found, wrong driver.
/// </summary>
public class EndTripTests : TripServiceBase
{
    // ─── Success ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Success_SetsTripStatusToEnded()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.EndTripAsync(tripId: 100, driverId: 1, Ct);

        // Assert
        Assert.Equal(TripStatus.Ended, trip.Status);
    }

    [Fact]
    public async Task Success_SetsEndedAt()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        var before = DateTime.UtcNow;

        // Act
        await Service.EndTripAsync(tripId: 100, driverId: 1, Ct);

        // Assert
        Assert.NotNull(trip.EndedAt);
        Assert.True(trip.EndedAt >= before);
    }

    [Fact]
    public async Task Success_PersistsChanges()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.EndTripAsync(tripId: 100, driverId: 1, Ct);

        // Assert
        Uow.Verify(x => x.CompleteAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task Success_RemovesDriverTripCacheKey()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);
        SetDriverTrip(driverId: 1, tripId: 100);

        // Act
        await Service.EndTripAsync(tripId: 100, driverId: 1, Ct);

        // Assert
        Assert.False(Cache.TryGetValue("driver-trip:1", out _), "driver-trip key must be removed");
    }

    [Fact]
    public async Task Success_RemovesLocationCacheKey()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);
        SetLocation(tripId: 100);

        // Act
        await Service.EndTripAsync(tripId: 100, driverId: 1, Ct);

        // Assert
        Assert.False(Cache.TryGetValue("bus-location:100", out _), "location key must be removed");
    }

    [Fact]
    public async Task Success_RemovesBothCacheKeys_AtOnce()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);
        SetDriverTrip(driverId: 1, tripId: 100);
        SetLocation(tripId: 100);

        // Act
        await Service.EndTripAsync(tripId: 100, driverId: 1, Ct);

        // Assert — both keys gone
        Assert.False(Cache.TryGetValue("driver-trip:1",    out _));
        Assert.False(Cache.TryGetValue("bus-location:100", out _));
    }

    // ─── Trip not found ───────────────────────────────────────────────────────

    [Fact]
    public async Task TripNotFound_ThrowsNotFoundException()
    {
        // Arrange
        TripRepo.Setup(x => x.GetActiveByIdAsync(999, Ct)).ReturnsAsync((TripEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.EndTripAsync(tripId: 999, driverId: 1, Ct));
    }

    [Fact]
    public async Task TripNotFound_NothingRemovedFromCache()
    {
        // Arrange
        TripRepo.Setup(x => x.GetActiveByIdAsync(999, Ct)).ReturnsAsync((TripEntity?)null);
        SetDriverTrip(driverId: 1, tripId: 100);

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.EndTripAsync(tripId: 999, driverId: 1, Ct));

        // Assert — cache untouched
        Assert.True(Cache.TryGetValue("driver-trip:1", out _));
    }

    // ─── Wrong driver ─────────────────────────────────────────────────────────

    [Fact]
    public async Task NotDriversTrip_ThrowsUnauthorizedException()
    {
        // Arrange — trip owned by driver 5
        var trip = MakeTrip(driverId: 5, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.EndTripAsync(tripId: 100, driverId: 1, Ct));
    }

    [Fact]
    public async Task NotDriversTrip_TripStatusRemainsActive()
    {
        // Arrange
        var trip = MakeTrip(driverId: 5, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.EndTripAsync(tripId: 100, driverId: 1, Ct));

        // Assert — trip not mutated
        Assert.Equal(TripStatus.Active, trip.Status);
        Assert.Null(trip.EndedAt);
        Uow.Verify(x => x.CompleteAsync(Ct), Times.Never);
    }
}
