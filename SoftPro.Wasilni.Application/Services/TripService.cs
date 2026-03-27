using Domain.Resources;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
using SoftPro.Wasilni.Application.Extensions;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Application.Services;

public class TripService(IUnitOfWork unitOfWork, IMemoryCache cache) : ITripService
{
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
                cache.Set(TripCacheKeys.DriverTrip(driverId), existing.Id);
                var loc = cache.Get<BusLocationModel>(TripCacheKeys.Location(existing.Id));
                return existing.ToModel(bus.Plate, bus.LineEntity.Name, loc);
            }
            throw new AlreadyExistsException(Phrases.BusAlreadyActive);
        }

        TripEntity trip = TripEntity.Create(busId, driverId, bus.LineId);
        await unitOfWork.TripRepository.AddAsync(trip, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Set(TripCacheKeys.DriverTrip(driverId), trip.Id);

        return trip.ToModel(bus.Plate, bus.LineEntity.Name, null);
    }

    public async Task EndTripAsync(int tripId, int driverId, CancellationToken cancellationToken)
    {
        TripEntity trip = await GetOwnedActiveTripAsync(tripId, driverId, cancellationToken);
        trip.End();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Remove(TripCacheKeys.DriverTrip(driverId));
        cache.Remove(TripCacheKeys.Location(tripId));
    }

    public async Task UpdateLocationAsync(int tripId, double latitude, double longitude, int driverId, CancellationToken cancellationToken)
    {
        // Fast path: validate from cache (0 DB operations)
        if (!cache.TryGetValue(TripCacheKeys.DriverTrip(driverId), out int cachedTripId) || cachedTripId != tripId)
        {
            // Fallback to DB (server restart / first update after reconnect)
            await GetOwnedActiveTripAsync(tripId, driverId, cancellationToken);
            cache.Set(TripCacheKeys.DriverTrip(driverId), tripId);
        }

        cache.Set(TripCacheKeys.Location(tripId), new BusLocationModel(latitude, longitude, DateTime.UtcNow));
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

        var location = cache.Get<BusLocationModel>(TripCacheKeys.Location(trip.Id));
        return trip.ToModel(trip.Bus.Plate, trip.Bus.LineEntity.Name, location);
    }

    // ─── Passenger Operations ─────────────────────────────────────────────────

    public async Task<List<GetTripModel>> GetActiveTripsAsync(int? lineId, CancellationToken cancellationToken)
    {
        List<TripEntity> trips = await unitOfWork.TripRepository.GetActiveTripsAsync(lineId, cancellationToken);

        return trips.Select(t =>
        {
            var location = cache.Get<BusLocationModel>(TripCacheKeys.Location(t.Id));
            return t.ToModel(t.Bus.Plate, t.Bus.LineEntity.Name, location);
        }).ToList();
    }

    public async Task<GetBookingModel> AddBookingAsync(int tripId, int passengerId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        TripEntity trip = await unitOfWork.TripRepository.GetActiveByIdAsync(tripId, cancellationToken)
            ?? throw new NotFoundException(Phrases.TripNotFound);

        bool alreadyBooked = await unitOfWork.BookingRepository
            .HasActiveBookingOnTripAsync(passengerId, tripId, cancellationToken);

        if (alreadyBooked)
            throw new AlreadyExistsException(Phrases.AlreadyBooked);

        BookingEntity booking = BookingEntity.Create(tripId, passengerId, latitude, longitude);
        await unitOfWork.BookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return booking.ToModel();
    }

    public async Task<int> CancelBookingAsync(int tripId, int passengerId, CancellationToken cancellationToken)
    {
        _ = await unitOfWork.TripRepository.GetActiveByIdAsync(tripId, cancellationToken)
            ?? throw new NotFoundException(Phrases.TripNotFound);

        BookingEntity booking = await unitOfWork.BookingRepository
            .GetActiveByPassengerAndTripAsync(passengerId, tripId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        if (booking.PassengerId != passengerId)
            throw new UnauthorizedException(Phrases.NotYourBooking);

        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);

        return booking.Id;
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
}
