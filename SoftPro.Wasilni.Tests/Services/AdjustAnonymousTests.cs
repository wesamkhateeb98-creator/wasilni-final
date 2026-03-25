using Moq;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Tests.Helpers;
using Xunit;

namespace SoftPro.Wasilni.Tests.Services;

/// <summary>
/// Tests for TripService.AdjustAnonymousAsync
/// Covers: increment, decrement, floor-at-zero, accumulation, persist, unauthorized.
/// </summary>
public class AdjustAnonymousTests : TripServiceBase
{
    // ─── Increment ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Increment_ReturnsOne_WhenStartingFromZero()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        int count = await Service.AdjustAnonymousAsync(tripId: 100, delta: +1, driverId: 1, Ct);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Increment_PersistsToDatabase()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.AdjustAnonymousAsync(tripId: 100, delta: +1, driverId: 1, Ct);

        // Assert
        Uow.Verify(x => x.CompleteAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task Increment_CountReflectedOnEntity()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.AdjustAnonymousAsync(100, +1, 1, Ct);

        // Assert — entity's AnonymousCount also updated (not just return value)
        Assert.Equal(1, trip.AnonymousCount);
    }

    // ─── Decrement ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Decrement_ReturnsZero_WhenAlreadyAtZero()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);  // AnonymousCount = 0
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        int count = await Service.AdjustAnonymousAsync(tripId: 100, delta: -1, driverId: 1, Ct);

        // Assert — Math.Max(0, 0 - 1) = 0
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Decrement_NeverGoesNegative_WithLargeDelta()
    {
        // Arrange — only 2 anonymous passengers, driver sends -10
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);
        await Service.AdjustAnonymousAsync(100, +2, 1, Ct);   // count → 2

        // Act
        int count = await Service.AdjustAnonymousAsync(100, -10, 1, Ct);

        // Assert — floored at zero
        Assert.Equal(0, count);
    }

    // ─── Accumulation ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MultipleIncrements_Accumulate()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.AdjustAnonymousAsync(100, +1, 1, Ct);
        await Service.AdjustAnonymousAsync(100, +1, 1, Ct);
        int count = await Service.AdjustAnonymousAsync(100, +1, 1, Ct);

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task IncrementThenDecrement_ReturnsCorrectBalance()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act — 3 board, 1 exits
        await Service.AdjustAnonymousAsync(100, +3, 1, Ct);
        int count = await Service.AdjustAnonymousAsync(100, -1, 1, Ct);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task IncrementDecrementToZero_ThenIncrementAgain_Works()
    {
        // Arrange
        var trip = MakeTrip(driverId: 1, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Service.AdjustAnonymousAsync(100, +2, 1, Ct);
        await Service.AdjustAnonymousAsync(100, -2, 1, Ct);
        int count = await Service.AdjustAnonymousAsync(100, +1, 1, Ct);

        // Assert
        Assert.Equal(1, count);
    }

    // ─── Unauthorized ─────────────────────────────────────────────────────────

    [Fact]
    public async Task NotDriversTrip_ThrowsUnauthorizedException()
    {
        // Arrange — trip owned by driver 5
        var trip = MakeTrip(driverId: 5, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.AdjustAnonymousAsync(tripId: 100, delta: +1, driverId: 1, Ct));
    }

    [Fact]
    public async Task NotDriversTrip_CountNotMutated()
    {
        // Arrange
        var trip = MakeTrip(driverId: 5, tripId: 100);
        TripRepo.Setup(x => x.GetActiveByIdAsync(100, Ct)).ReturnsAsync(trip);

        // Act
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Service.AdjustAnonymousAsync(100, +1, driverId: 1, Ct));

        // Assert — entity untouched
        Assert.Equal(0, trip.AnonymousCount);
        Uow.Verify(x => x.CompleteAsync(Ct), Times.Never);
    }

    // ─── Trip not found ───────────────────────────────────────────────────────

    [Fact]
    public async Task TripNotFound_ThrowsNotFoundException()
    {
        // Arrange
        TripRepo.Setup(x => x.GetActiveByIdAsync(999, Ct)).ReturnsAsync((TripEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.AdjustAnonymousAsync(tripId: 999, delta: +1, driverId: 1, Ct));
    }
}
