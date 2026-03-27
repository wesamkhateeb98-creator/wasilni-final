namespace SoftPro.Wasilni.Presentation.Models.Response.Trip;

public record GetBookingResponse(
    int Id,
    int TripId,
    int PassengerId,
    double Latitude,
    double Longitude,
    string Status,
    DateTime CreatedAt);
