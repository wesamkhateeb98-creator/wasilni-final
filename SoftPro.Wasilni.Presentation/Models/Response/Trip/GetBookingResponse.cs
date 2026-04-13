namespace SoftPro.Wasilni.Presentation.Models.Response.Trip;

public record GetBookingResponse(
    int      Id,
    int      LineId,
    int      PassengerId,
    string   PassengerName,
    DateOnly Date,
    double   Latitude,
    double   Longitude,
    string   Status,
    DateTime CreatedAt);
