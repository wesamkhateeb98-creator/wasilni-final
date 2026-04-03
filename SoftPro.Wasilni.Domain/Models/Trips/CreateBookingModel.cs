namespace SoftPro.Wasilni.Domain.Models.Trips;

public record CreateBookingModel(int LineId, int PassengerId, double Latitude, double Longitude);
