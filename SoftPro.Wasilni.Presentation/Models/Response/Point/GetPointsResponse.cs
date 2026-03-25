namespace SoftPro.Wasilni.Presentation.Models.Response.Point;

public record GetPointsResponse(int Id, double Latitude, double Longitude, int? LineId);

