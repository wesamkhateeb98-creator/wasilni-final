namespace SoftPro.Wasilni.Domain.Models.Reports;

public record GetDailyRidershipModel(int BusId, int LineId, DateOnly Day, int NumberOfRiders);
