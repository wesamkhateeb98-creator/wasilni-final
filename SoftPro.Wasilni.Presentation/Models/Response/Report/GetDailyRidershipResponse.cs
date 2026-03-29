namespace SoftPro.Wasilni.Presentation.Models.Response.Report;

public record GetDailyRidershipResponse(int BusId, int LineId, DateOnly Day, int NumberOfRiders);
