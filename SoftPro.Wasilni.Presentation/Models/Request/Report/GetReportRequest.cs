using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Report;

public record GetReportRequest(
    ReportType  Type,
    DateTime    From,
    DateTime    To,
    int?        LineId,
    int?        BusId);
