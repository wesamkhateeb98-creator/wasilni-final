using Domain.Resources;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Services;

public class TripService(IUnitOfWork unitOfWork, IMemoryCache cache) : ITripService
{
    // ─── Cache Keys ───────────────────────────────────────────────────────────

    private static string LocationKey(int tripId)   => $"bus-location:{tripId}";
    private static string DriverTripKey(int driverId) => $"driver-trip:{driverId}";

    // ─── Driver Operations ────────────────────────────────────────────────────

    public async Task<GetTripModel> StartTripAsync(int busId, int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdWithLineAsync(busId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId != driverId)
            throw new UnauthorizedException(Phrases.NotYourBus);

        // Handle reconnect: active trip already exists for this driver
        TripEntity? existing = await unitOfWork.TripRepository.GetActiveTripByBusIdAsync(busId, cancellationToken);
        if (existing is not null)
        {
            if (existing.DriverId == driverId)
            {
                // Reconnect → restore cache and return existing trip
                cache.Set(DriverTripKey(driverId), existing.Id);
                var loc = cache.Get<BusLocationModel>(LocationKey(existing.Id));
                return ToModel(existing, bus.Plate, bus.LineEntity.Name, loc);
            }
            throw new AlreadyExistsException(Phrases.BusAlreadyActive);
        }

        TripEntity trip = TripEntity.Create(busId, driverId, bus.LineId);
        await unitOfWork.TripRepository.AddAsync(trip, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Set(DriverTripKey(driverId), trip.Id);

        return ToModel(trip, bus.Plate, bus.LineEntity.Name, null);
    }

    public async Task EndTripAsync(int tripId, int driverId, CancellationToken cancellationToken)
    {
        TripEntity trip = await GetOwnedActiveTripAsync(tripId, driverId, cancellationToken);
        trip.End();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Remove(DriverTripKey(driverId));
        cache.Remove(LocationKey(tripId));
    }

    public async Task UpdateLocationAsync(int tripId, double latitude, double longitude, int driverId, CancellationToken cancellationToken)
    {
        // Fast path: validate from cache (0 DB operations)
        if (!cache.TryGetValue(DriverTripKey(driverId), out int cachedTripId) || cachedTripId != tripId)
        {
            // Fallback to DB (server restart / first update after reconnect)
            await GetOwnedActiveTripAsync(tripId, driverId, cancellationToken);
            cache.Set(DriverTripKey(driverId), tripId);
        }

        cache.Set(LocationKey(tripId), new BusLocationModel(latitude, longitude, DateTime.UtcNow));
    }

    public async Task<int> AdjustAnonymousAsync(int tripId, int delta, int driverId, CancellationToken cancellationToken)
    {
        TripEntity trip = await GetOwnedActiveTripAsync(tripId, driverId, cancellationToken);
        trip.AdjustAnonymous(delta);
        await unitOfWork.CompleteAsync(cancellationToken);
        return trip.AnonymousCount;
    }

    public async Task<GetTripModel?> GetMyActiveTripAsync(int driverId, CancellationToken cancellationToken)
    {
        TripEntity? trip = await unitOfWork.TripRepository.GetActiveTripByDriverIdAsync(driverId, cancellationToken);
        if (trip is null) return null;

        var location = cache.Get<BusLocationModel>(LocationKey(trip.Id));
        return ToModel(trip, trip.Bus.Plate, trip.Bus.LineEntity.Name, location);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<TripEntity> GetOwnedActiveTripAsync(int tripId, int driverId, CancellationToken cancellationToken)
    {
        TripEntity trip = await unitOfWork.TripRepository.GetActiveByIdAsync(tripId, cancellationToken)
            ?? throw new NotFoundException(Phrases.TripNotFound);

        if (trip.DriverId != driverId)
            throw new UnauthorizedException(Phrases.NotYourBus);

        return trip;
    }

    private static GetTripModel ToModel(TripEntity trip, string busPlate, string lineName, BusLocationModel? location)
        => new(trip.Id, trip.BusId, busPlate, trip.LineId, lineName, trip.Status,
               location?.Latitude, location?.Longitude, trip.AnonymousCount, trip.StartedAt);
}
