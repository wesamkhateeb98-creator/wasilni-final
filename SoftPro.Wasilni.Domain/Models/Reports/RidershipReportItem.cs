namespace SoftPro.Wasilni.Domain.Models.Reports;

public record RidershipReportItem(
    int?      LineId,
    int?      BusId,
    int?      Year,
    int?      Month,
    DateOnly? Day,
    int       TotalRiders);
