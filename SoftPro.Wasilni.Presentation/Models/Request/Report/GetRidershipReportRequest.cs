using SoftPro.Wasilni.Domain.Enums;

public record GetRidershipReportRequest(
    ReportType Type,
    DateTime From,
    DateTime To);