using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Extensions;

public static class TripEntityExtensions
{
    public static GetTripModel ToModel(
        this TripEntity      trip,
        string               busPlate,
        string               lineName,
        BusLocationModel?    location)
        => new(trip.Id,
               trip.BusId,
               busPlate,
               trip.LineId,
               lineName,
               trip.Status,
               location?.Latitude,
               location?.Longitude,
               trip.AnonymousCount,
               trip.StartedAt);

    public static GetBookingModel ToModel(this BookingEntity booking)
        => new(booking.Id,
               booking.TripId,
               booking.PassengerId,
               booking.Latitude,
               booking.Longitude,
               booking.Status,
               booking.CreatedAt);
}
