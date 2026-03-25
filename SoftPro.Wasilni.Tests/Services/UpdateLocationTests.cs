using Microsoft.Extensions.Caching.Memory;
using Moq;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Tests.Helpers;
using Xunit;

namespace SoftPro.Wasilni.Tests.Services;

/// <summary>
/// Tests for TripService.UpdateLocationAsync
/// Covers: cache-hit fast-path (0 DB), cache-miss DB fallback, stale tripId in cache, unauthorized.
/// </summary>
public class UpdateLocationTests : TripServiceBase
{
    // ─── Cache-hit fast path ──────────────────────────────────────────────────

    [Fact]
    public async Task CacheHit_WritesNewLocationToCache()
    {
        // Arrange
        SetDriverTrip(driverId: 1, tripId: 100);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5138, longitude: 36.2765, driverId: 1, Ct);

        // Assert
        var loc = Cache.Get<BusLocationModel>("bus-location:100");
        Assert.NotNull(loc);
        Assert.Equal(33.5138, loc.Latitude);
        Assert.Equal(36.2765, loc.Longitude);
    }

    [Fact]
    public async Task CacheHit_DoesNotCallDatabase()
    {
        // Arrange
        SetDriverTrip(driverId: 1, tripId: 100);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert — zero DB operations
        TripRepo.Verify(x => x.GetActiveByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        Uow.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CacheHit_OverwritesPreviousLocation()
    {
        // Arrange
        SetDriverTrip(driverId: 1, tripId: 100);
        SetLocation(tripId: 100, lat: 10.0, lng: 20.0);   // old location

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert — cache updated with new coords
        var loc = Cache.Get<BusLocationModel>("bus-location:100");
        Assert.NotNull(loc);
        Assert.Equal(33.5, loc.Latitude);
        Assert.Equal(36.3, loc.Longitude);
    }

    [Fact]
    public async Task CacheHit_LocationTimestampIsRecent()
    {
        // Arrange
        SetDriverTrip(driverId: 1, tripId: 100);
        var before = DateTime.UtcNow;

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert
        var loc = Cache.Get<BusLocationModel>("bus-location:100");
        Assert.True(loc!.UpdatedAt >= before);
    }

    // ─── Cache-miss → DB fallback ─────────────────────────────────────────────

    [Fact]
    public async Task CacheMiss_FallsBackToDatabase()
    {
        // Arrange — nothing in cache (server restart / first update)
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert — DB was queried once
        TripRepo.Verify(x => x.GetActiveByIdAsync(100, Ct), Times.Once);
    }

    [Fact]
    public async Task CacheMiss_SetsLocationInCache()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert
        Assert.NotNull(Cache.Get<BusLocationModel>("bus-location:100"));
    }

    [Fact]
    public async Task CacheMiss_RestoresDriverTripCacheKey()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert — subsequent updates will hit the fast path
        Assert.True(Cache.TryGetValue("driver-trip:1", out int id));
        Assert.Equal(100, id);
    }

    // ─── Stale tripId in cache ────────────────────────────────────────────────

    [Fact]
    public async Task StaleTripIdInCache_FallsBackToDatabase()
    {
        // Arrange — cache holds a different tripId (e.g. previous trip)
        SetDriverTrip(driverId: 1, tripId: 999);   // stale
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert
        TripRepo.Verify(x => x.GetActiveByIdAsync(100, Ct), Times.Once);
    }

    [Fact]
    public async Task StaleTripIdInCache_UpdatesCacheKeyToNewTripId()
    {
        // Arrange
        SetDriverTrip(driverId: 1, tripId: 999);
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct);

        // Assert — cache corrected to the current trip
        Assert.True(Cache.TryGetValue("driver-trip:1", out int id));
        Assert.Equal(100, id);
    }

    // ─── Unauthorized ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CacheMiss_TripOwnedByAnotherDriver_ThrowsUnauthorizedException()
    {
        // Arrange — trip belongs to driver 5, driver 1 sends updates
        var trip = MakeTrip(driverId: 5, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct));
    }

    [Fact]
    public async Task Unauthorized_LocationNotWrittenToCache()
    {
        // Arrange
        var trip = MakeTrip(driverId: 5, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.UpdateLocationAsync(tripId: 100, latitude: 33.5, longitude: 36.3, driverId: 1, Ct));

        // Assert — cache not polluted
        Assert.Null(Cache.Get<BusLocationModel>("bus-location:100"));
    }
}
