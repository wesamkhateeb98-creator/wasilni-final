using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Report;

public record GetReportFilterRequest(
    ReportType Type,
    DateTime   From,
    DateTime   To);
