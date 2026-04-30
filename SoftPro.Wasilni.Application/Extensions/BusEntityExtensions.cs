using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Extensions;

public static class BusEntityExtensions
{
    public static GetActiveBusModel ToModel(this BusEntity bus, BusLocationModel? location)
        => new(bus.Id,
               bus.Plate,
               bus.LineId!.Value,
               bus.LineEntity!.Name,
               bus.Status,
               location?.Latitude,
               location?.Longitude,
               bus.ActiveSince);

    public static GetBookingModel ToModel(this BookingEntity booking)
        => new(booking.Id,
               booking.LineId,
               booking.PassengerId,
               booking.Passenger is null
                    ? string.Empty
                    : string.Join(" ", new[] { booking.Passenger.FirstName, booking.Passenger.LastName }
                        .Where(value => !string.IsNullOrWhiteSpace(value))),
               booking.Date,
               booking.Latitude,
               booking.Longitude,
               booking.Status,
               booking.CreatedAt);
}
