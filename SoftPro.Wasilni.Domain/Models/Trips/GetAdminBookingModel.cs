using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Trips;

public record GetAdminBookingModel(
    int           BookingId,
    int           PassengerId,
    string        PassengerName,
    int           LineId,
    DateOnly      Date,
    double        Latitude,
    double        Longitude,
    BookingStatus Status,
    DateTime      CreatedAt);
