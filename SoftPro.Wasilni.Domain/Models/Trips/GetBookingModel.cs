using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Trips;

public record GetBookingModel(
    int Id,
    int TripId,
    int PassengerId,
    double Latitude,
    double Longitude,
    BookingStatus Status,
    DateTime CreatedAt);
