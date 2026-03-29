namespace SoftPro.Wasilni.Domain.Models.Reports;

public record RidershipReportItem(
    int?      BusId,
    int?      LineId,
    int?      Year,
    int?      Month,
    DateOnly? Day,
    int       TotalRiders);
