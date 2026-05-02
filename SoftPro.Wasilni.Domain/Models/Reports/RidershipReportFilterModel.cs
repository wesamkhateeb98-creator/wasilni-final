using SoftPro.Wasilni.Domain.Enums;

public record RidershipReportFilterModel(
    ReportType Type,
    DateTime From,
    DateTime To,
    int? LineId,
    int? BusId);