namespace SoftPro.Wasilni.Presentation.Models.Response.Trip;

public record GetAdminBookingResponse(
    int      BookingId,
    int      PassengerId,
    string   PassengerName,
    int      LineId,
    string   LineName,
    DateOnly Date,
    double   Latitude,
    double   Longitude,
    string   Status,
    DateTime CreatedAt);
