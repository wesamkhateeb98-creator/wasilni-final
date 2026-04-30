namespace SoftPro.Wasilni.Domain.Models.Reports;

public record DailyRidershipData(int LineId, int BusId, DateOnly Day, int NumberOfRiders);